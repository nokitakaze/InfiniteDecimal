using System;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace InfiniteDecimal;

public partial class BigDec
{
    public override bool Equals(object obj)
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
        return decimal.Parse(item.ToStringDouble());
    }

    public static explicit operator double(BigDec item)
    {
        return double.Parse(item.ToStringDouble());
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
            var p = GetPow10BigInt(maxOffset - a._offset);
            valueA *= p;
        }

        BigInteger valueB = b.Value;
        if (b._offset < maxOffset)
        {
            var p = GetPow10BigInt(maxOffset - b._offset);
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
        if (a == Zero)
        {
            return Zero;
        }

        if (b == Zero)
        {
            throw new InfiniteDecimalException("Division by zero");
        }

        if (b == One)
        {
            return a.WithPrecision(Math.Max(a.MaxPrecision, b.MaxPrecision));
        }

        if (a == b)
        {
            return One;
        }

        var desiredPrecision = Math.Max(a.MaxPrecision, b.MaxPrecision);
        var realLocalPrecision = Math.Max(desiredPrecision, Math.Max(a.Offset, b.Offset));
        var result = new BigDec(a, realLocalPrecision);
        if (result._offset < result.MaxPrecision * 2)
        {
            var awaitedPrecision = result.MaxPrecision * 10;
            var addExp = awaitedPrecision - result._offset;
            result.Value *= GetPow10BigInt(addExp);
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