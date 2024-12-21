using System.Numerics;

namespace InfiniteDecimal.Test;

public class SimplePrecisionTest
{
    #region Precision copy

    [Fact]
    public void Copy_WithPrecision()
    {
        var a = new BigDec(1337);
        {
            var b1 = a.WithPrecision(a.MaxPrecision + 2);
            b1 += 1;
            Assert.NotEqual(a, b1);
        }
        {
            var b2 = a.WithPrecision(a.MaxPrecision);
            b2 += 1;
            Assert.NotEqual(a, b2);
        }
    }

    [Fact]
    public void Copy_Copy()
    {
        var a = new BigDec(1337);
        var b = a;
        b += 1;

        Assert.NotEqual(a, b);
    }

    [Fact]
    public void Copy_Round()
    {
        var a = new BigDec(1337);
        {
            var b1 = a.Round(a.MaxPrecision + 2);
            b1 += 1;
            Assert.NotEqual(a, b1);
        }
        {
            var b2 = a.Round(a.MaxPrecision);
            b2 += 1;
            Assert.NotEqual(a, b2);
        }
    }

    #endregion

    #region

    [Fact]
    public void TestDeletionTrailingZeros()
    {
        for (var n2 = 0; n2 < 5; n2++)
        {
            var body2 = BigInteger.Pow(2, n2);

            for (var n5 = 0; n5 < 5; n5++)
            {
                var body5 = BigInteger.Pow(5, n5);
                var value = new BigDec(body2) * 0.0001m * new BigDec(body5);
                if (value.Offset > 0)
                {
                    Assert.NotEqual(0, value.BigIntegerBody % 10);
                }
            }

            {
                var body5 = BigInteger.Pow(5, n2);
                var value = new BigDec(body2) * 0.0001m * new BigDec(body5);
                Assert.Equal(1, value.BigIntegerBody);
            }
        }
    }

    [Fact]
    public void TestReduced()
    {
        var value = new BigDec(1) + BigDec.PowFractionOfTen(18);

        for (var i = 0; i < 18; i++)
        {
            var actual1 = value.WithPrecision(i);
            Assert.Equal(1, actual1.BigIntegerBody);
            Assert.Equal(0, actual1.Offset);
        }
    }

    #endregion

    // TODO A * 0 => precision copy
}
