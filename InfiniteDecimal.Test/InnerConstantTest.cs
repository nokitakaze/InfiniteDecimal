namespace InfiniteDecimal.Test;

public class InnerConstantTest
{
    private static readonly BigDec Epsilon = BigDec.One.WithPrecision(998) / BigDec.BigInt10Powers[998];

    public static object[][] TestEData()
    {
        return new object[][]
        {
            new object[] { BigDec.E_Root8, 2, BigDec.E_Root4 },
            new object[] { BigDec.E_Root8, 4, BigDec.E_Sqrt },
            new object[] { BigDec.E_Root8, 8, BigDec.E },
            new object[] { BigDec.E_Root4, 2, BigDec.E_Sqrt },
            new object[] { BigDec.E_Root4, 4, BigDec.E },
            new object[] { BigDec.E_Sqrt, 2, BigDec.E },
        };
    }

    [Theory]
    [MemberData(nameof(TestEData))]
    public void TestE(BigDec baseValue, int exp, BigDec expected)
    {
        var actual = baseValue.Pow(exp);

        var minValue = expected - Epsilon;
        var maxValue = expected + Epsilon;
        Assert.InRange(actual, minValue, maxValue);
    }
}