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
        return System.HashCode.Combine(this._mantissa, this.MaxPrecision);
    }

    #region operator type casting

    public static explicit operator BigInteger(BigDec item)
    {
        return item._mantissa / item.OffsetPower;
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

    public static explicit operator decimal(BigDec item)
    {
        if (item == Zero)
        {
            return 0m;
        }

        var scale = item.Offset;
        if (scale == 0)
        {
            // If we got "overflow here" System.Numberic will raise it anyway
            return (decimal)item._mantissa;
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

        var isNegative = item._mantissa < 0;
        var value = BigInteger.Abs(item._mantissa);
        if (scale > MaxDecimalScale)
        {
            value /= Pow10BigInt(scale - MaxDecimalScale);
            scale = MaxDecimalScale;
        }

        while ((scale > 0) && (GetRealByteCount(value) > 12))
        {
            value /= BigInteger10;
            scale--;
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
        return double.Parse(item.ToStringDouble(CultureInfo.InvariantCulture), CultureInfo.InvariantCulture);
    }

    public static explicit operator float(BigDec item)
    {
        // TODO Maybe we need to do it more lower way
        return float.Parse(item.ToStringDouble(CultureInfo.InvariantCulture), CultureInfo.InvariantCulture);
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

        return (a.Offset == b.Offset) && (a._mantissa == b._mantissa);
    }

    public static bool operator >(BigDec a, BigDec b)
    {
        if (a == b)
        {
            return false;
        }

        if (b == Zero)
        {
            return a._mantissa > 0;
        }

        if ((a._mantissa < 0) != (b._mantissa < 0))
        {
            return (a._mantissa >= 0);
        }

        return (a - b)._mantissa > 0;
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
        var newValue = new BigDec(-a._mantissa, a.Offset, a.OffsetPower, a.MaxPrecision);

        return newValue;
    }

    public static BigDec operator +(BigDec a, BigDec b)
    {
        var maxOffset = Math.Max(a.Offset, b.Offset);

        BigInteger valueA = a._mantissa;
        if (a.Offset < maxOffset)
        {
            var p = Pow10BigInt(maxOffset - a.Offset);
            valueA *= p;
        }

        BigInteger valueB = b._mantissa;
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
        var mantissa = a._mantissa * b._mantissa;
        var maxOffset = a.Offset + b.Offset;
        var offsetPower = Pow10BigInt(maxOffset);
        var maxPrecision = Math.Max(a.MaxPrecision, b.MaxPrecision);

        ReduceOverflowPrecision(ref mantissa, ref maxOffset, ref offsetPower, maxPrecision);
        var newValue = new BigDec(mantissa, maxOffset, offsetPower, maxPrecision: maxPrecision);

        return newValue;
    }

    public static BigDec operator *(BigDec a, BigInteger b)
    {
        var mantissa = a._mantissa * b;
        var maxOffset = a.Offset;
        var offsetPower = a.OffsetPower;

        ReduceOverflowPrecision(ref mantissa, ref maxOffset, ref offsetPower, a.MaxPrecision);
        var newValue = new BigDec(mantissa, maxOffset, offsetPower, maxPrecision: a.MaxPrecision);

        return newValue;
    }

    #endregion

    #region operator /

    public static BigDec operator /(BigDec a, BigDec b)
    {
        if (b.IsZero)
        {
            throw new InfiniteDecimalException("Division by zero");
        }

        if (a.IsZero)
        {
            return Zero;
        }

        var desiredPrecision = Math.Max(a.MaxPrecision, b.MaxPrecision);

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

        // if (result.Offset < result_maxPrecision * 2) // always true condition
        {
            var awaitedPrecision = result_maxPrecision * 10;
            var addExp = awaitedPrecision - result_offset;
            result__mantissa *= Pow10BigInt(addExp);
            result_offset = awaitedPrecision;
        }

        result__mantissa /= b._mantissa;
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
