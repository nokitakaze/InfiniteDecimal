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
                    Assert.NotEqual(0, value.Mantissa % 10);
                }
            }

            {
                var body5 = BigInteger.Pow(5, n2);
                var value = new BigDec(body2) * 0.0001m * new BigDec(body5);
                Assert.Equal(1, value.Mantissa);
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
            Assert.Equal(1, actual1.Mantissa);
            Assert.Equal(0, actual1.Offset);
        }
    }

    [Fact]
    public void TestRoundReducesBody()
    {
        var value = new BigDec(1) + BigDec.PowFractionOfTen(18);

        for (var i = 0; i < 18; i++)
        {
            var actual1 = value.Round(i);
            Assert.Equal(1, actual1.Mantissa);
            Assert.Equal(0, actual1.Offset);
        }
    }


    public static object[][] TestRounding_Round_Data()
    {
        var input = new (decimal value, int precision)[]
        {
            (1.00001m, 6),
            (1.00001m, 5),
            (1.00001m, 4),
            (1.00001m, 3),
            (1.00001m, 0),
            (1.00005m, 4),
            (1.0000500001m, 5),
            (1.0000500001m, 4),
            (1.0000500001m, 3),
            (1.0000500001m, 0),
            (1.0001500001m, 5),
            (1.0001500001m, 4),
            (1.0001500001m, 3),
            (1.0001500001m, 0),
            (1.00005m, 5),
            (1.00005m, 4),
            (1.00005m, 3),
            (1.00005m, 0),
            (1.00015m, 5),
            (1.00015m, 4),
            (1.00015m, 3),
            (1.00015m, 0),
            (1.4m, 0),
            (1.5m, 0),
            (1.501m, 0),
            (1.499m, 0),
            (1.499m, 1),
            (1.499m, 2),
            (2.4m, 0),
            (2.5m, 0),
            (2.501m, 0),
            (2.499m, 0),
            (2.499m, 1),
            (2.499m, 2),
        };

        return input
            .Select(item =>
            {
                var power = (decimal)BigDec.Pow10BigInt(item.precision);
                var floor = Math.Floor(item.value * power) / power;

                return new object[]
                {
                    new BigDec(item.value),
                    item.precision,
                    new BigDec(Math.Round(item.value, item.precision, MidpointRounding.ToEven)),
                    new BigDec(floor)
                };
            })
            .ToArray();
    }

    [Theory]
    [MemberData(nameof(TestRounding_Round_Data))]
    public void TestRounding_Round(BigDec input, int precision, BigDec expected, BigDec _)
    {
        var actual = input.Round(precision);
        Assert.Equal(expected, actual);
        Assert.NotEqual(0, actual.Mantissa % 10);
    }

    [Theory]
    [MemberData(nameof(TestRounding_Round_Data))]
    public void TestRounding_Floor(BigDec input, int precision, BigDec _, BigDec expected)
    {
        var actual = input.Floor(precision);
        Assert.Equal(expected, actual);
        Assert.NotEqual(0, actual.Mantissa % 10);
        actual = input.WithPrecision(precision);
        Assert.Equal(expected, actual);
        Assert.NotEqual(0, actual.Mantissa % 10);
        actual = new BigDec(input, precision);
        Assert.Equal(expected, actual);
        Assert.NotEqual(0, actual.Mantissa % 10);
    }

    #endregion

    // TODO A * 0 => precision copy
}
