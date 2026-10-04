using System;
using System.Globalization;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace InfiniteDecimal;

public partial class BigDec
{
    public override bool Equals(object? obj)
    {
        if (obj is not BigDec other)
        {
            return false;
        }

        return (this == other);
    }

    public override int GetHashCode()
    {
        // ReSharper disable once NonReadonlyMemberInGetHashCode
        return System.HashCode.Combine(this.Mantissa, this.Offset);
    }

    #region operator type casting

    public static explicit operator BigInteger(BigDec item)
    {
        return item.Mantissa / item.OffsetPower;
    }

    public static explicit operator int(BigDec item)
    {
        return (int)(BigInteger)item;
    }

    public static explicit operator long(BigDec item)
    {
        return (long)(BigInteger)item;
    }

    public static explicit operator ulong(BigDec item)
    {
        return (ulong)(BigInteger)item;
    }

    /// <summary>
    /// Ln(10) / Ln(2)
    /// </summary>
    protected const double Log2_10 = 3.321928094887362347870319429489d;

    public static explicit operator decimal(BigDec item)
    {
        if (item == Zero)
        {
            return 0m;
        }

        var scale = item.Offset;
        if (scale == 0)
        {
            // If we got "overflow here" System.Numeric will raise it anyway
            return (decimal)item.Mantissa;
        }

        if (item > 0)
        {
            if ((item > MaxDecimalValue) || (item < MinAbsDecimalValue))
            {
                throw new OverflowException("Value was either too large or too small for a Decimal");
            }
        }
        else
        {
            if ((item < -MaxDecimalValue) || (item > -MinAbsDecimalValue))
            {
                throw new OverflowException("Value was either too large or too small for a Decimal");
            }
        }

        var isNegative = (item.Mantissa.Sign == -1);
        var value = BigInteger.Abs(item.Mantissa);
        if (((scale > 0) && (GetRealByteCount(value) > 12)) || (scale > MaxDecimalScale))
        {
            var digitCount = BigInteger.Log10(value);
            var needCropDigits = Math.Max(
                (int)Math.Ceiling(digitCount - MaxDecimalScale),
                scale - MaxDecimalScale
            );
            needCropDigits = Math.Min(needCropDigits, scale);

            // https://learn.microsoft.com/en-us/dotnet/api/system.decimal.parse?view=net-10.0
            // "rounding to nearest"
            var denominator = Pow10BigInt(needCropDigits);
            var remainder = value % denominator;
            var half = Pow10BigInt(needCropDigits - 1) * 5;
            value /= denominator;
            if (remainder > half)
            {
                value++;
            }
            else if (remainder == half)
            {
                if (!value.IsEven)
                {
                    value++;
                }
            }

            scale -= needCropDigits;
        }

        var mask = (BigInteger.One << 32) - 1;
        uint uByteLo = (uint)(value & mask);
        var byteLo = unchecked((int)uByteLo);
        uint uByteMid = (uint)((value >> 32) & mask);
        var byteMid = unchecked((int)uByteMid);
        uint uByteHi = (uint)((value >> 64) & mask);
        var byteHi = unchecked((int)uByteHi);

        var result = new decimal(byteLo, byteMid, byteHi, isNegative, (byte)scale);
        return result;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GetRealByteCount(BigInteger item)
    {
        if (item.IsZero)
        {
            return 1;
        }

        return BigInteger
            .Abs(item)
            // ReSharper disable once RedundantArgumentDefaultValue
            .ToByteArray(true, false)
            .Length;
    }

    public static explicit operator double(BigDec item)
    {
        return double.Parse(item.ToStringDouble(CultureInfo.InvariantCulture.NumberFormat),
            CultureInfo.InvariantCulture);
    }

    public static explicit operator float(BigDec item)
    {
        // TODO Maybe we need to do it more lower way
        return Convert.ToSingle((double)item);
    }

    #endregion

    #region operator ==

    public static bool operator ==(BigDec? a, BigDec? b)
    {
        if ((a is null) && (b is null))
        {
            return true;
        }

        if ((a is null) || (b is null))
        {
            return false;
        }

        return (a.Offset == b.Offset) && (a.Mantissa == b.Mantissa);
    }

    public static bool operator >(BigDec a, BigDec b)
    {
        if (a == b)
        {
            return false;
        }

        if (b == Zero)
        {
            return a.Mantissa > 0;
        }

        if ((a.Mantissa < 0) != (b.Mantissa < 0))
        {
            return (a.Mantissa >= 0);
        }

        return (a - b).Mantissa > 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator <(BigDec a, BigDec b)
    {
        return (b > a);
    }

    #endregion

    #region operator +-

    /// <summary>
    /// Unary minus
    /// </summary>
    /// <param name="a"></param>
    /// <returns></returns>
    public static BigDec operator -(BigDec a)
    {
        var newValue = new BigDec(-a.Mantissa, a.Offset, a.OffsetPower, a.MaxPrecision);

        return newValue;
    }

    public static BigDec operator +(BigDec a, BigDec b)
    {
        var maxOffset = Math.Max(a.Offset, b.Offset);

        BigInteger valueA = a.Mantissa;
        if (a.Offset < maxOffset)
        {
            var p = Pow10BigInt(maxOffset - a.Offset);
            valueA *= p;
        }

        BigInteger valueB = b.Mantissa;
        if (b.Offset < maxOffset)
        {
            var p = Pow10BigInt(maxOffset - b.Offset);
            valueB *= p;
        }

        var mantissa = valueA + valueB;
        var maxPrecision = Math.Max(a.MaxPrecision, b.MaxPrecision);
        var offsetPower = Pow10BigInt(maxOffset);
        ReduceOverflowPrecision(ref mantissa, ref maxOffset, ref offsetPower, maxPrecision);
        var newValue = new BigDec(mantissa, maxOffset, offsetPower, maxPrecision: maxPrecision);

        return newValue;
    }

    public static BigDec operator -(BigDec a, BigDec b)
    {
        return a + (-b);
    }

    public static BigDec operator +(BigDec a, BigInteger b)
    {
        var (mantissa, maxOffset, offsetPower, maxPrecision) = new BigDec(a);
        mantissa += b * offsetPower;
        // hint: По идее здесь это делать не нужно, ведь мы целое число прибавляем
        ReduceOverflowPrecision(ref mantissa, ref maxOffset, ref offsetPower, maxPrecision);
        var newValue = new BigDec(mantissa, maxOffset, offsetPower, maxPrecision: maxPrecision);

        return newValue;
    }

    #endregion

    #region operator *

    public static BigDec operator *(BigDec a, BigDec b)
    {
        var mantissa = a.Mantissa * b.Mantissa;
        var maxOffset = a.Offset + b.Offset;
        var offsetPower = Pow10BigInt(maxOffset);
        var maxPrecision = Math.Max(a.MaxPrecision, b.MaxPrecision);

        ReduceOverflowPrecision(ref mantissa, ref maxOffset, ref offsetPower, maxPrecision);
        var newValue = new BigDec(mantissa, maxOffset, offsetPower, maxPrecision: maxPrecision);

        return newValue;
    }

    public static BigDec operator *(BigDec a, BigInteger b)
    {
        var mantissa = a.Mantissa * b;
        var maxOffset = a.Offset;
        var offsetPower = a.OffsetPower;

        ReduceOverflowPrecision(ref mantissa, ref maxOffset, ref offsetPower, a.MaxPrecision);
        var newValue = new BigDec(mantissa, maxOffset, offsetPower, maxPrecision: a.MaxPrecision);

        return newValue;
    }

    #endregion

    #region operator /

    /// <summary>
    /// Dividing two numbers. Division rounds the result down by the absolute value when there is not enough precision
    /// </summary>
    /// <remarks>Division rounds the result, just as BigInteger do</remarks>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    /// <exception cref="DivideByZeroException"></exception>
    /// <exception cref="OutOfMemoryException"></exception>
    /// <exception cref="InfiniteDecimalException"></exception>
    public static BigDec operator /(BigDec a, BigDec b)
    {
        if (b.IsZero)
        {
            throw new DivideByZeroException($"Division {a} by zero");
        }

        var desiredPrecision = Math.Max(a.MaxPrecision, b.MaxPrecision);
        if (a.IsZero)
        {
            return Zero.WithPrecision(desiredPrecision);
        }

        if (b == One)
        {
            return a.WithPrecision(desiredPrecision);
        }

        if (a == One)
        {
            return b.WithPrecision(desiredPrecision).Inverse();
        }

        if (a == b)
        {
            return One.WithPrecision(desiredPrecision);
        }

        var (result__mantissa, result_offset, _, result_maxPrecision) = a;
        result_maxPrecision = Math.Max(result_maxPrecision, b.MaxPrecision);

        {
            // 2^31 bits / (ln(10)/ln(2)) = 646_456_993 decimal digits
            if (result_maxPrecision >= 64_645_699L)
            {
                long t = result_maxPrecision * 10L;
                throw new OutOfMemoryException($"Awaited precision ({t:N0}) is too big");
            }

            var awaitedPrecision = result_maxPrecision * 10;
            var addExp = awaitedPrecision - result_offset;
            result__mantissa *= Pow10BigInt(addExp);
            result_offset = awaitedPrecision;
        }

        result__mantissa /= b.Mantissa;
        result_offset -= b.Offset;
        // codecov ignore start
        if (result_offset < 0)
        {
            throw new InfiniteDecimalException("Precision from arguments didn't apply to result");
        }
        // codecov ignore end

        var offsetPower = Pow10BigInt(result_offset);
        ReduceOverflowPrecision(ref result__mantissa, ref result_offset, ref offsetPower, result_maxPrecision);
        var result = new BigDec(result__mantissa, result_offset, offsetPower, result_maxPrecision);

        return result.Round(desiredPrecision);
    }

    #endregion
}
