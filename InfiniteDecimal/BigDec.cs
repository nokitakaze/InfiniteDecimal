using System;
using System.Collections.Generic;
using System.Globalization;
using System.Numerics;
using System.Text.RegularExpressions;

namespace InfiniteDecimal;

public partial class BigDec
{
    public const int MaxDefaultPrecision = 18;
    public static readonly BigInteger BigInteger10 = new BigInteger(10);
    public readonly int MaxPrecision = MaxDefaultPrecision;
    public static readonly BigDec One = new BigDec(1);
    public static readonly BigDec Zero = new BigDec(0);

    protected BigInteger Value = BigInteger.Zero;
    protected int _offset;

    protected int Offset
    {
        get => _offset;
        set
        {
            if (value < 0)
            {
                throw new InfiniteDecimalException($"Offset ('{value}') < 0");
            }

            if (_offset == value)
            {
                return;
            }

            _offset = value;
            OffsetPower = Pow10BigInt(_offset);
        }
    }

    protected BigInteger OffsetPower = BigInteger.One;
    public bool IsInteger => (_offset == 0);

    public static readonly IReadOnlyDictionary<int, BigInteger> BigInt10Powers;

    static BigDec()
    {
        {
            var powDec = new Dictionary<int, BigInteger>();
            var last = BigInteger.One;
            for (var i = 1; i <= 2000; i++)
            {
                last *= BigInteger10;
                powDec[i] = last;
            }

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
    }

    /// <summary>
    /// Calculate the power of 10 raised to the given exponent
    /// </summary>
    /// <param name="exp">The exponent</param>
    /// <returns>The result of 10 raised to the power of <paramref name="exp"/>.</returns>
    public static BigInteger Pow10BigInt(int exp)
    {
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
            return BigInteger.Pow(new BigInteger(10), exp);
        }
    }

    /// <summary>
    /// Calculates the power of 0.1
    /// </summary>
    /// <param name="power">The power to raise 0.1 to</param>
    /// <param name="maxPrecision">The maximum precision to use.</param>
    /// <returns>The result of raising 0.1 to the specified power.</returns>
    public static BigDec PowFracOfTen(int power, int maxPrecision = MaxDefaultPrecision)
    {
        if (power <= 0)
        {
            return new BigDec(BigInteger.Pow(BigInteger10, -power));
        }

        var result = One.WithPrecision(Math.Max(power, maxPrecision));
        result.Offset = power;

        return result;
    }

    public void NormalizeOffset()
    {
        if (Value.IsZero)
        {
            Offset = 0;
            return;
        }

        if (_offset == 0)
        {
            return;
        }

        var value = (Value > 0) ? Value : -Value;
        var u = false;
        while ((_offset > 0) && (value % BigInteger10 == BigInteger.Zero))
        {
            _offset--;
            value /= 10;
            Value /= 10;
            u = true;
        }

        if (u)
        {
            OffsetPower = Pow10BigInt(_offset);
        }
    }

    /// <summary>
    /// Represents a number in string format, which resembles the format of a floating-point number,
    /// taking into account cultural settings
    /// </summary>
    /// <param name="cultureInfo"></param>
    /// <returns></returns>
    public string ToStringDouble(CultureInfo? cultureInfo = null)
    {
        if (_offset == 0)
        {
            return Value.ToString();
        }

        if (OffsetPower.IsZero)
        {
            throw new InfiniteDecimalException($"Inconsistent state in BidDec. Offset '{_offset}' malformed");
        }

        var sign = string.Empty;
        var _value = Value;
        if (Value < 0)
        {
            sign = "-";
            _value = -_value;
        }

        var vTail = _value % OffsetPower;
        var vEntier = (_value - vTail) / OffsetPower;

        var vTailString = string.Empty;
        for (var i = 0; i < _offset; i++)
        {
            var mode = (int)(_value % BigInteger10);
            vTailString = mode + vTailString;
            _value /= BigInteger10;
        }

        vTailString = vTailString.TrimEnd('0');
        if (vTailString == string.Empty)
        {
            return $"{sign}{vEntier}";
        }

        string separator = ".";
        if (cultureInfo != null)
        {
            separator = cultureInfo.NumberFormat.NumberDecimalSeparator;
        }

        return $"{sign}{vEntier}{separator}{vTailString}";
    }

    public override string ToString()
    {
        return ToStringDouble(CultureInfo.InvariantCulture);
    }

    public string ToString(IFormatProvider provider)
    {
        if (provider is CultureInfo cultureInfo)
        {
            return ToStringDouble(cultureInfo);
        }

        return ToStringDouble();
    }

    /// <summary>
    /// Regular expression for matching exponential notation strings.
    /// The regex matches strings in the format: [+/-][digits].[digits]e[+/-][digits]
    /// For example: 1.23e+10
    /// </summary>
    private static Regex? ExponentialNotationRegex;

    public static BigDec Parse(string value)
    {
        if (value == "0")
        {
            return Zero;
        }

        value = value
            .Replace(" ", string.Empty)
            .Replace("_", string.Empty);

        {
            ExponentialNotationRegex ??= new Regex("^([+-]?[0-9]+(?:\\.[0-9]*?)?)e([+-][0-9]+)$");
            var m = ExponentialNotationRegex.Match(value);
            if (m.Success)
            {
                var body = Parse(m.Groups[1].Value);
                var exp = int.Parse(m.Groups[2].Value);
                var frac = PowFracOfTen(-exp);

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
            var bigInteger10 = new BigInteger(10);

            var tail = BigInteger.Zero;
            for (var i = 0; i < chunks1.Length; i++)
            {
                tail *= bigInteger10;
                var c1 = chunks1.Substring(i, 1);
                tail += int.Parse(c1);
            }

            // ReSharper disable once UseObjectOrCollectionInitializer
            var valBI = new BigDec(tail);
            valBI._offset = chunks1.Length;
            valBI.OffsetPower = Pow10BigInt(valBI._offset);
            valBI.Value += valBI.OffsetPower * BigInteger.Parse(chunks[0]);
            valBI.Value *= sign;
            if (valBI._offset > valBI.MaxPrecision)
            {
                valBI = valBI.WithPrecision(valBI._offset);
            }

            return valBI;
        }
    }
}