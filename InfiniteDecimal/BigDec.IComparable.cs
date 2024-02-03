using System.Numerics;

namespace InfiniteDecimal;

public partial class BigDec : System.IComparable
{
    public int CompareTo(object? obj)
    {
        if (obj is null)
        {
            // Comparing with null returns ONE.
            // 10.CompareTo(null) = 1
            return 1;
        }

        BigDec? other = obj switch
        {
            BigDec pure => pure,
            BigInteger pure => new BigDec(pure),
            byte vByte => new BigDec(vByte),
            sbyte vSbyte => new BigDec(vSbyte),
            ushort vUshort => new BigDec(vUshort),
            short vShort => new BigDec(vShort),
            uint vUint => new BigDec(vUint),
            int vInt => new BigDec(vInt),
            ulong vUlong => new BigDec(vUlong),
            long vLong => new BigDec(vLong),
            decimal vDecimal => new BigDec(vDecimal),
            double vDouble => new BigDec(vDouble),
            _ => null
        };

        if (other is null)
        {
            throw new InfiniteDecimalException($"Can't compare with {obj.GetType()}");
        }

        if (this == other)
        {
            return 0;
        }
        else if (this < other)
        {
            return -1;
        }
        else
        {
            return 1;
        }
    }
}