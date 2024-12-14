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
        return System.HashCode.Combine(this.Value, this.MaxPrecision);
    }

    #region operator type casting

    public static explicit operator BigInteger(BigDec item)
    {
        return item.Value / item.OffsetPower;
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
            return (decimal)item.Value;
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

        var isNegative = item.Value < 0;
        var value = BigInteger.Abs(item.Value);
        if (scale > MaxDecimalScale)
        {
            value /= BigInteger.Pow(BigInteger10, scale - MaxDecimalScale);
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

    public static int GetRealByteCount(BigInteger item)
    {
        if (item.IsZero)
        {
            return 1;
        }

        var array = BigInteger.Abs(item).ToByteArray(true, false);
        for (var count = array.Length - 1; count >= 0; count--)
        {
            if (array[count] > 0)
            {
                return count + 1;
            }
        }

        throw new Exception();
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

        a.NormalizeOffset();
        b.NormalizeOffset();
        return (a._offset == b._offset) && (a.Value == b.Value);
    }

    public static bool operator >(BigDec a, BigDec b)
    {
        if (a == b)
        {
            return false;
        }

        if (b == Zero)
        {
            return a.Value > 0;
        }

        if ((a.Value < 0) != (b.Value < 0))
        {
            return (a.Value >= 0);
        }

        return (a - b).Value > 0;
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
        // a.NormalizeOffset();
        var newValue = new BigDec(a);
        newValue.Value = -newValue.Value;

        return newValue;
    }

    public static BigDec operator +(BigDec a, BigDec b)
    {
        var maxOffset = Math.Max(a._offset, b._offset);

        BigInteger valueA = a.Value;
        if (a._offset < maxOffset)
        {
            var p = Pow10BigInt(maxOffset - a._offset);
            valueA *= p;
        }

        BigInteger valueB = b.Value;
        if (b._offset < maxOffset)
        {
            var p = Pow10BigInt(maxOffset - b._offset);
            valueB *= p;
        }

        var newValue = new BigDec(a, Math.Max(a.MaxPrecision, b.MaxPrecision))
        {
            Value = valueA + valueB,
            Offset = maxOffset,
        };
        newValue.NormalizeOffset();

        return newValue;
    }

    public static BigDec operator -(BigDec a, BigDec b)
    {
        return a + (-b);
    }

    public static BigDec operator +(BigDec a, BigInteger b)
    {
        var newValue = new BigDec(a);
        newValue.Value += b * newValue.OffsetPower;
        newValue.NormalizeOffset();

        return newValue;
    }

    #endregion

    #region operator *

    public static BigDec operator *(BigDec a, BigDec b)
    {
        var newValue = a.WithPrecision(Math.Max(a.MaxPrecision, b.MaxPrecision));
        newValue.Offset += b.Offset;
        newValue.Value *= b.Value;
        newValue.NormalizeOffset();

        return newValue;
    }

    public static BigDec operator *(BigDec a, BigInteger b)
    {
        var newValue = new BigDec(a);
        newValue.Value *= b;
        newValue.NormalizeOffset();

        return newValue;
    }

    #endregion

    #region operator /

    public static BigDec operator /(BigDec a, BigDec b)
    {
        if (a.IsZero)
        {
            return Zero;
        }

        if (b.IsZero)
        {
            throw new InfiniteDecimalException("Division by zero");
        }

        var desiredPrecision = Math.Max(a.MaxPrecision, b.MaxPrecision);

        if (b == One)
        {
            return a.WithPrecision(desiredPrecision);
        }

        if (a == b)
        {
            return One.WithPrecision(desiredPrecision);
        }

        var realLocalPrecision = Math.Max(desiredPrecision, Math.Max(a.Offset, b.Offset));
        var result = new BigDec(a, realLocalPrecision);
        // if (result._offset < result.MaxPrecision * 2) // always true condition
        {
            var awaitedPrecision = result.MaxPrecision * 10;
            var addExp = awaitedPrecision - result._offset;
            result.Value *= Pow10BigInt(addExp);
            result.Offset = awaitedPrecision;
        }

        result.Value /= b.Value;
        var newOffset = result._offset - b._offset;
        // codecov ignore start
        if (newOffset < 0)
        {
            throw new InfiniteDecimalException("Precision from arguments didn't apply to result");
        }
        // codecov ignore end

        result.Offset = newOffset;

        return result.Round(desiredPrecision);
    }

    #endregion
}
