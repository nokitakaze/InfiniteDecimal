using System.Runtime.CompilerServices;

namespace InfiniteDecimal;

public partial class BigDec
{
    #region Operators

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(BigDec a, decimal b)
    {
        return (a == new BigDec(b));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(decimal b, BigDec a)
    {
        return (a == new BigDec(b));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(BigDec a, decimal b)
    {
        return (a != new BigDec(b));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(decimal b, BigDec a)
    {
        return (a != new BigDec(b));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator >(BigDec a, decimal b)
    {
        return (a > new BigDec(b));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator >(decimal b, BigDec a)
    {
        return (new BigDec(b) > a);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator <=(BigDec a, decimal b)
    {
        return (a <= new BigDec(b));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator <=(decimal b, BigDec a)
    {
        return (new BigDec(b) <= a);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator <(BigDec a, decimal b)
    {
        return (a < new BigDec(b));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator <(decimal b, BigDec a)
    {
        return (new BigDec(b) < a);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator >=(BigDec a, decimal b)
    {
        return (a >= new BigDec(b));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator >=(decimal b, BigDec a)
    {
        return (new BigDec(b) >= a);
    }

    #endregion

    #region Math operations

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BigDec operator +(BigDec a, decimal b)
    {
        return a + new BigDec(b);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BigDec operator +(decimal b, BigDec a)
    {
        return new BigDec(b) + a;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BigDec operator -(BigDec a, decimal b)
    {
        return a - new BigDec(b);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BigDec operator -(decimal b, BigDec a)
    {
        return new BigDec(b) - a;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BigDec operator /(BigDec a, decimal b)
    {
        return a / new BigDec(b);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BigDec operator /(decimal b, BigDec a)
    {
        return new BigDec(b) / a;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BigDec operator *(BigDec a, decimal b)
    {
        return a * new BigDec(b);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BigDec operator *(decimal b, BigDec a)
    {
        return new BigDec(b) * a;
    }

    #endregion
}
