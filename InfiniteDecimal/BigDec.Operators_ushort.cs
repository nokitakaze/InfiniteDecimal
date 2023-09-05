using System;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace InfiniteDecimal;

public partial class BigDec
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(BigDec a, ushort b)
    {
        return (a == new BigDec(b));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(ushort b, BigDec a)
    {
        return (a == new BigDec(b));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(BigDec a, ushort b)
    {
        return (a != new BigDec(b));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(ushort b, BigDec a)
    {
        return (a != new BigDec(b));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator >(BigDec a, ushort b)
    {
        return (a > new BigDec(b));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator >(ushort b, BigDec a)
    {
        return (new BigDec(b) > a);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator <=(BigDec a, ushort b)
    {
        return (a <= new BigDec(b));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator <=(ushort b, BigDec a)
    {
        return (new BigDec(b) <= a);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator <(BigDec a, ushort b)
    {
        return (a < new BigDec(b));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator <(ushort b, BigDec a)
    {
        return (new BigDec(b) < a);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator >=(BigDec a, ushort b)
    {
        return (a >= new BigDec(b));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator >=(ushort b, BigDec a)
    {
        return (new BigDec(b) >= a);
    }
}