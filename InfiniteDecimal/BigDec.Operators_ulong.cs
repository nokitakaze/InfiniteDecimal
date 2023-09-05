using System.Runtime.CompilerServices;

namespace InfiniteDecimal;

public partial class BigDec
{
    #region Operators

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(BigDec a, ulong b)
    {
        return (a == new BigDec(b));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(ulong b, BigDec a)
    {
        return (a == new BigDec(b));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(BigDec a, ulong b)
    {
        return (a != new BigDec(b));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(ulong b, BigDec a)
    {
        return (a != new BigDec(b));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator >(BigDec a, ulong b)
    {
        return (a > new BigDec(b));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator >(ulong b, BigDec a)
    {
        return (new BigDec(b) > a);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator <=(BigDec a, ulong b)
    {
        return (a <= new BigDec(b));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator <=(ulong b, BigDec a)
    {
        return (new BigDec(b) <= a);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator <(BigDec a, ulong b)
    {
        return (a < new BigDec(b));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator <(ulong b, BigDec a)
    {
        return (new BigDec(b) < a);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator >=(BigDec a, ulong b)
    {
        return (a >= new BigDec(b));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator >=(ulong b, BigDec a)
    {
        return (new BigDec(b) >= a);
    }

    #endregion

    #region Math operations

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BigDec operator +(BigDec a, ulong b)
    {
        return a + new BigDec(b);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BigDec operator +(ulong b, BigDec a)
    {
        return new BigDec(b) + a;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BigDec operator -(BigDec a, ulong b)
    {
        return a - new BigDec(b);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BigDec operator -(ulong b, BigDec a)
    {
        return new BigDec(b) - a;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BigDec operator /(BigDec a, ulong b)
    {
        return a / new BigDec(b);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BigDec operator /(ulong b, BigDec a)
    {
        return new BigDec(b) / a;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BigDec operator *(BigDec a, ulong b)
    {
        return a * new BigDec(b);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BigDec operator *(ulong b, BigDec a)
    {
        return new BigDec(b) * a;
    }

    #endregion
}