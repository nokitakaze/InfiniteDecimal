using System;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace InfiniteDecimal;

public partial class BigDec
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(BigDec a, BigDec b)
    {
        return !(a == b);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator <=(BigDec a, BigDec b)
    {
        return !(a > b);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator <(BigDec a, BigDec b)
    {
        return !(a >= b);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator >=(BigDec a, BigDec b)
    {
        return !(a < b);
    }
}