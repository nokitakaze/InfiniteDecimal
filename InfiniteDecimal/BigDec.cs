using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Text.RegularExpressions;

namespace InfiniteDecimal;

public partial class BigDec
{
    public const int MaxDefaultPrecision = 18;

    /// <summary>
    /// Regular expression for matching exponential notation strings.
    /// The regex matches strings in the format: [+/-][digits].[digits]e[+/-][digits]
    /// For example: 1.23e+10
    /// </summary>
    private static readonly Regex ExponentialNotationRegex = new Regex(
        "^([+-]?[0-9]+(?:\\.[0-9]*?)?)e([+-][0-9]+)$",
        RegexOptions.Compiled
    );

    /// <summary>
    /// A constant BigInteger representing the numeric value ten, used as the base for decimal scaling
    /// and exponentiation operations within the BigDec class
    /// </summary>
    // ReSharper disable once MemberInitializerValueIgnored
    public static BigInteger BigInteger10 => BigDecConstant.BigInteger10;

    /// <summary>
    /// Specifies the maximum number of decimal digits that the BigDec instance can accurately represent
    /// and maintain for its fractional part
    /// </summary>
    public readonly int MaxPrecision = MaxDefaultPrecision;

    public static readonly BigDec One = new BigDec(1);
    public static readonly BigDec Zero = new BigDec(0);
    public static readonly BigDec Half = new BigDec(0.5m);

    /// <summary>
    /// Represents the core numeric value of the decimal as a BigInteger, without considering its decimal offset
    /// </summary>
    protected readonly BigInteger _mantissa = BigInteger.Zero;

    /// <summary>
    /// Represents the scale of the decimal value by specifying how many digits are placed to the right
    /// of the decimal point. An offset of zero implies the value is an integer, while a positive offset shifts
    /// the decimal point accordingly
    /// </summary>
    public readonly int Offset;

    /// <summary>
    /// Represents the core numeric value of the decimal as a BigInteger, without considering its decimal offset
    /// </summary>
    public BigInteger Mantissa => _mantissa;

    /// <summary>
    /// Represents the numeric scale factor associated with the current offset, calculated as 10
    /// raised to the power of `Offset`
    /// </summary>
    /// <remarks>
    /// It is used internally to efficiently handle scaling operations and avoid recalculating powers
    /// of ten during arithmetic computations.
    /// </remarks>
    protected readonly BigInteger OffsetPower = BigInteger.One;

    /// <summary>
    /// Indicates whether this BigDec instance represents an integer value, i.e., a value without any fractional component.
    /// </summary>
    public bool IsInteger => (Offset == 0);

    /// <summary>
    /// A read-only dictionary that maps an exponent to the corresponding power of 10 as a BigInteger.
    /// </summary>
    /// <remarks>
    /// It provides fast access to precomputed values of 10^n to avoid recalculating them repeatedly.
    /// </remarks>
    public static readonly IReadOnlyDictionary<int, BigInteger> BigInt10Powers;

    static BigDec()
    {
        E_inverse = 1m / E;
        {
            var powDec = new Dictionary<int, BigInteger>();
            var last = BigInteger.One;
            for (var i = 1; i <= 2000; i++)
            {
                last *= BigInteger10;
                powDec[i] = last;
            }

            powDec[3000] = powDec[1000] * powDec[2000];
            BigInt10Powers = powDec;
        }
        /*
        {
            E_Sqrt = E.Sqrt().WithPrecision(E.MaxPrecision);
            E_Root4 = E_Sqrt.Sqrt().WithPrecision(E.MaxPrecision);
            E_Root8 = E_Root4.Sqrt().WithPrecision(E.MaxPrecision);
            E_Root16 = E_Root8.Sqrt().WithPrecision(E.MaxPrecision);
            E_Root32 = E_Root16.Sqrt().WithPrecision(E.MaxPrecision);
        }
        // */
        ExpModifiers = GenerateExpModifiers();
        ExpModifiers_multipliers = ExpModifiers.Select(t => (double)t.multiplier).ToArray();
        ExpModifiers_exp = ExpModifiers.Select(t => t.exp).ToArray();
        MaxDecimalValue = new((BigInteger.One << 96) - 1);
        MinAbsDecimalValue = new BigDec(BigInteger.One, MaxDecimalScale, MaxDecimalScale);
    }

    /// <summary>
    /// Calculate the power of 10 raised to the given exponent
    /// </summary>
    /// <param name="exp">The exponent</param>
    /// <returns>The result of 10 raised to the power of <paramref name="exp"/>.</returns>
    public static BigInteger Pow10BigInt(int exp)
    {
        // 2^31 bits / (ln(10)/ln(2)) = 646_456_993 decimal digits
        if (exp > 646_456_993)
        {
            throw new OutOfMemoryException($"Awaited exponent ({exp:N0}) of ten is too big");
        }

        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if ((BigInt10Powers is not null) && BigInt10Powers.TryGetValue(exp, out var t))
        {
            return t;
        }
        else if (!BigInteger10.IsZero)
        {
            return BigInteger.Pow(BigInteger10, exp);
        }
        else
        {
            // Calling a method during static construction
            return BigInteger.Pow(new BigInteger(10), exp);
        }
    }

    /// <summary>
    /// Calculates the power of 0.1
    /// </summary>
    /// <param name="power">The power to raise 0.1 to</param>
    /// <param name="maxPrecision">The maximum precision to use.</param>
    /// <returns>The result of raising 0.1 to the specified power.</returns>
    public static BigDec PowFractionOfTen(int power, int maxPrecision = MaxDefaultPrecision)
    {
        // ReSharper disable once ConvertIfStatementToReturnStatement
        if (power <= 0)
        {
            return new BigDec(BigDec.Pow10BigInt(-power), maxPrecision);
        }

        return new BigDec(BigInteger.One, power, Math.Max(power, maxPrecision));
    }

    #region Reduce complexity

    /// <summary>
    /// Reduce the offset while the body is divisible by 10
    /// </summary>
    public static void ReduceTrailingZeroes(ref BigInteger mantissa, ref int offset, ref BigInteger OffsetPower)
    {
        if (mantissa.IsZero)
        {
            offset = 0;
            OffsetPower = BigInteger.One;
            return;
        }

        if (offset == 0)
        {
            return;
        }

        var value = (mantissa > 0) ? mantissa : -mantissa;
        if (!(value % BigInteger10).IsZero)
        {
            return;
        }

        var s1 = value.ToString();
        var s2 = s1.TrimEnd('0');
        var addOffset = Math.Min(s1.Length - s2.Length, offset);
        var denominator = Pow10BigInt(addOffset);
        offset -= addOffset;
        mantissa /= denominator;
        OffsetPower = Pow10BigInt(offset);
    }

    /// <summary>
    /// Reduce the offset while the body is divisible by 10
    /// </summary>
    public static void ReduceTrailingZeroesWOPower(ref BigInteger mantissa, ref int offset, out BigInteger OffsetPower)
    {
        if (mantissa.IsZero)
        {
            offset = 0;
            OffsetPower = BigInteger.One;
            return;
        }

        if (offset == 0)
        {
            OffsetPower = BigInteger.One;
            return;
        }

        var value = (mantissa > 0) ? mantissa : -mantissa;
        if (!(value % BigInteger10).IsZero)
        {
            OffsetPower = Pow10BigInt(offset);
            return;
        }

        var s1 = value.ToString();
        var s2 = s1.TrimEnd('0');
        var addOffset = Math.Min(s1.Length - s2.Length, offset);
        var denominator = Pow10BigInt(addOffset);
        offset -= addOffset;
        mantissa /= denominator;
        OffsetPower = Pow10BigInt(offset);
    }

    /// <summary>
    /// Reducing Offset if it exceeds Max Precision
    /// </summary>
    public static void ReduceOverflowPrecision(
        ref BigInteger mantissa,
        ref int offset,
        ref BigInteger offsetPower,
        int maxPrecision
    )
    {
        AssertPrecision(maxPrecision, nameof(maxPrecision));
        var expDiff = offset - maxPrecision;
        if (expDiff <= 0)
        {
            ReduceTrailingZeroes(ref mantissa, ref offset, ref offsetPower);
            return;
        }

        var denominator = Pow10BigInt(expDiff);
        mantissa /= denominator;
        offset = maxPrecision;
        offsetPower = Pow10BigInt(offset);
        ReduceTrailingZeroes(ref mantissa, ref offset, ref offsetPower);
    }

    #endregion

    /// <summary>
    /// Represents a number in string format, which resembles the format of a floating-point number,
    /// taking into account cultural settings
    /// </summary>
    /// <param name="cultureInfo"></param>
    /// <param name="numberFormatInfo"></param>
    /// <returns></returns>
    public string ToStringDouble(
        CultureInfo? cultureInfo = null,
        NumberFormatInfo? numberFormatInfo = null
    )
    {
        if (Offset == 0)
        {
            // It's just a big integer
            return _mantissa.ToString();
        }

        var sign = string.Empty;
        var _value = _mantissa;
        if (_mantissa < 0)
        {
            sign = "-";
            _value = -_value;
        }

        var vTail = _value % OffsetPower;
        var vEntier = (_value - vTail) / OffsetPower;

        var vTailString = string.Empty;
        for (var i = 0; i < Offset; i++)
        {
            var mode = (int)(_value % BigInteger10);
            // ReSharper disable once RedundantToStringCallForValueType
            vTailString = mode.ToString() + vTailString;
            _value /= BigInteger10;
        }

        vTailString = vTailString.TrimEnd('0');
        if (vTailString == string.Empty) // always false condition
        {
            return $"{sign}{vEntier}";
        }

        string separator = ".";
        if (cultureInfo != null)
        {
            separator = cultureInfo.NumberFormat.NumberDecimalSeparator;
        }
        else if (numberFormatInfo != null)
        {
            separator = numberFormatInfo.NumberDecimalSeparator;
        }

        return $"{sign}{vEntier}{separator}{vTailString}";
    }

    public override string ToString()
    {
        return ToStringDouble(CultureInfo.InvariantCulture);
    }

    public string ToString(IFormatProvider provider)
    {
        return provider switch
        {
            CultureInfo cultureInfo => ToStringDouble(cultureInfo: cultureInfo),
            NumberFormatInfo numberFormatInfo => ToStringDouble(numberFormatInfo: numberFormatInfo),
            _ => ToStringDouble(),
        };
    }

    public static BigDec Parse(string value)
    {
        // ReSharper disable once ConvertIfStatementToSwitchStatement
        if (value is null)
        {
            throw new ArgumentNullException(nameof(value));
        }

        if (value == "0")
        {
            return Zero;
        }

        value = value
            .Replace(" ", string.Empty)
            .Replace("_", string.Empty);

        {
            // This method could be called before static initialization
            // ReSharper disable NullCoalescingConditionIsAlwaysNotNullAccordingToAPIContract
            var exponentialNotationRegex = ExponentialNotationRegex ?? new Regex(
                "^([+-]?[0-9]+(?:\\.[0-9]*?)?)e([+-][0-9]+)$",
                RegexOptions.Compiled
            );
            // ReSharper restore NullCoalescingConditionIsAlwaysNotNullAccordingToAPIContract

            var m = exponentialNotationRegex.Match(value);
            if (m.Success)
            {
                var body = Parse(m.Groups[1].Value);
                if (body.IsZero)
                {
                    // poison exponent
                    return body;
                }

                var exp = int.Parse(m.Groups[2].Value);
                var frac = PowFractionOfTen(-exp);

                return body.WithPrecision(frac.Offset + body.MaxPrecision) * frac;
            }
        }

        var sign = 1;
        if (value.StartsWith("-"))
        {
            sign = -1;
            value = value[1..];
        }

        var chunks = value.Split('.');
        // ReSharper disable once ConvertIfStatementToSwitchStatement
        if (chunks.Length > 2)
        {
            throw new InfiniteDecimalException($"Value '{value}' malformed");
        }

        if (chunks.Length == 1)
        {
            var valBI = BigInteger.Parse(value) * sign;
            return new BigDec(valBI);
        }

        var chunks1 = chunks[1].TrimEnd('0');
        if (chunks1 == string.Empty)
        {
            var valBI = BigInteger.Parse(chunks[0]) * sign;
            return new BigDec(valBI);
        }

        {
            var newOffset = chunks1.Length;
            var valBI__mantissa = BigInteger.Parse(chunks[0] + chunks1, NumberStyles.Integer);
            if (sign == -1)
            {
                valBI__mantissa = -valBI__mantissa;
            }

            var maxPrecision = Math.Max(MaxDefaultPrecision, newOffset);
            var valBI = new BigDec(valBI__mantissa, newOffset, maxPrecision);

            return valBI;
        }
    }
}
