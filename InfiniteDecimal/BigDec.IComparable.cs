namespace InfiniteDecimal;

public partial class BigDec : System.IComparable
{
    public int CompareTo(object obj)
    {
        BigDec? other = obj switch
        {
            BigDec pure => pure,
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
            return -1;
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