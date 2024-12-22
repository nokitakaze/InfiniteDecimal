namespace InfiniteDecimal.Test;

public class InnerConstantTest
{
    private static readonly BigDec Epsilon = BigDec.PowFractionOfTen(996);

    public static ICollection<object[]> TestEData()
    {
        var constants = new BigDec[]
        {
            BigDec.E_Root1024,
            BigDec.E_Root512,
            BigDec.E_Root256,
            BigDec.E_Root128,
            BigDec.E_Root64,
            BigDec.E_Root32,
            BigDec.E_Root16,
            BigDec.E_Root8,
            BigDec.E_Root4,
            BigDec.E_Sqrt,
            BigDec.E,
        };

        var result = new List<object[]>();
        for (var i = 1; i < constants.Length; i++)
        {
            for (var j = 0; j < i; j++)
            {
                var exp = Math.Pow(2, i - j);
                result.Add(new object[] { constants[j], exp, constants[i] });
            }
        }

        return result;
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