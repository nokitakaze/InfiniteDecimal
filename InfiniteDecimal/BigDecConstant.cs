using System.Numerics;

namespace InfiniteDecimal;

public static class BigDecConstant
{
    /// <summary>
    /// A constant BigInteger representing the numeric value ten, used as the base for decimal scaling
    /// and exponentiation operations within the BigDec class
    /// </summary>
    // ReSharper disable once MemberInitializerValueIgnored
    public static readonly BigInteger BigInteger10 = new BigInteger(10);
}
