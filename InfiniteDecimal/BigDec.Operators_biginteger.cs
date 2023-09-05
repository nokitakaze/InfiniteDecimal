using System;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace InfiniteDecimal;

public partial class BigDec
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(BigDec a, BigInteger b)
    {
        return (a == new BigDec(b));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(BigInteger b, BigDec a)
    {
        return (a == new BigDec(b));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(BigDec a, BigInteger b)
    {
        return (a != new BigDec(b));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(BigInteger b, BigDec a)
    {
        return (a != new BigDec(b));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator >(BigDec a, BigInteger b)
    {
        return (a > new BigDec(b));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator >(BigInteger b, BigDec a)
    {
        return (new BigDec(b) > a);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator <=(BigDec a, BigInteger b)
    {
        return (a <= new BigDec(b));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator <=(BigInteger b, BigDec a)
    {
        return (new BigDec(b) <= a);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator <(BigDec a, BigInteger b)
    {
        return (a < new BigDec(b));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator <(BigInteger b, BigDec a)
    {
        return (new BigDec(b) < a);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator >=(BigDec a, BigInteger b)
    {
        return (a >= new BigDec(b));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator >=(BigInteger b, BigDec a)
    {
        return (new BigDec(b) >= a);
    }
}