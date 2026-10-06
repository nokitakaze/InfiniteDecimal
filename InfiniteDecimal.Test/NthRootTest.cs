using System.Numerics;

namespace InfiniteDecimal.Test;

public class NthRootTest
{
    public static IEnumerable<object[]> TestNthRoot_Data()
    {
        return Enumerable
            .Range(2, 19)
            .Select(i => new object[] { i });
    }

    [Theory]
    [MemberData(nameof(TestNthRoot_Data))]
    public void TestNthRoot(int degree)
    {
        const int power = 100;
        var epsilon = new BigDec(BigInteger.One, offset: power - 5, maxPrecision: power);

        for (decimal based = 2m; based < 1000m; based++)
        {
            var basedBI = new BigInteger(based) * BigInteger.Pow(10, power * degree);
            var root = BigDec.CalculateNthRoot(basedBI, degree);

            var rootBD = new BigDec(root, offset: power, maxPrecision: power);
            var regrowth = rootBD.Pow(degree);

            Assert.InRange(regrowth, new BigDec(based) - epsilon, new BigDec(based) + epsilon);
        }
    }

    [Fact]
    public void NegativeBase()
    {
        Assert.Throws<InfiniteDecimalException>(() => BigDec.CalculateNthRoot(BigInteger.MinusOne, 2));
        Assert.Throws<InfiniteDecimalException>(() => BigDec.CalculateNthRoot(-BigDec.BigInteger10, 2));
    }

    [Fact]
    public void TestBadDegree()
    {
        Assert.Throws<InfiniteDecimalException>(() => BigDec.CalculateNthRoot(BigDec.BigInteger10, 0));
        Assert.Throws<InfiniteDecimalException>(() => BigDec.CalculateNthRoot(BigDec.BigInteger10, 1));
    }

    [Fact]
    public void SmallBase()
    {
        for (int degree = 2; degree < 10; degree++)
        {
            Assert.Equal(BigInteger.Zero, BigDec.CalculateNthRoot(BigInteger.Zero, degree));
            Assert.Equal(BigInteger.One, BigDec.CalculateNthRoot(BigInteger.One, degree));
        }
    }
}
