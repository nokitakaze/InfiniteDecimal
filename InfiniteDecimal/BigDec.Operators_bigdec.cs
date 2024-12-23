using System.Runtime.CompilerServices;

namespace InfiniteDecimal;

public partial class BigDec
{
    #region Operators

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(BigDec? a, BigDec? b)
    {
        return !(a == b);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator <=(BigDec a, BigDec b)
    {
        return !(a > b);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator >=(BigDec a, BigDec b)
    {
        return !(a < b);
    }

    #endregion

    #region Math operations

    #endregion
}
