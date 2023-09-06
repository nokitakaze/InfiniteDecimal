namespace InfiniteDecimal.Test;

public class SqrtPowTest
{
    public static decimal[] GetTestDecimal()
    {
        var values = new decimal[]
        {
            0,
            1,
            2,
            (decimal)Math.E,
            (decimal)Math.PI,
            1.337m,
            15m,
            1234m,
            1.000_000_01m,
            0.000_000_01m,
            911,
            228,
        };

        return values
            .Concat(values.Select(t => t - 1))
            .Concat(values.Select(t => t + 1))
            .Distinct()
            .Where(x => x >= 0)
            .OrderBy(t => t)
            .ToArray();
    }

    public static object[][] TestSqrtData()
    {
        var values = GetTestDecimal();

        return values.Select(value => new object[] { value }).ToArray();
    }

    [Theory]
    [MemberData(nameof(TestSqrtData))]
    public void TestSqrt(decimal input)
    {
        if (input < 0)
        {
            return;
        }

        var powExpected = input * input;
        var powActual = (new BigDec(input)).Pow(2);
        Assert.True(powExpected == powActual);

        var actual1 = powActual.Sqrt();
        if (TrivialMathTests.IsInt(input))
        {
            Assert.Equal(new BigDec(input), actual1);
        }
        else
        {
            var diff = (actual1 - input).Abs();
            Assert.True(diff < 0.000_000_1m);
        }

        var actual2 = powActual.Pow(0.5m);
        throw new NotImplementedException();
    }

    [Theory]
    [MemberData(nameof(TestSqrtData))]
    public void TestLn(decimal input)
    {
        if (input <= 0)
        {
            return;
        }

        var expectedDouble = Math.Log((double)input);
        var expected = new BigDec(expectedDouble);
        var actual = new BigDec(input).Ln();

        var diff = (actual - expected).Abs();
        Assert.True(diff < 0.000_000_1m);

        var actualExp = actual.Exp();
        diff = (actualExp - input).Abs();
        Assert.True(diff < 0.000_000_1m);
    }

    [Fact]
    public void ThrowLnOnZero()
    {
        Assert.Throws<InfiniteDecimalException>(() => { BigDec.Zero.Ln(); });
    }

    [Fact]
    public void ThrowLnOnNegative()
    {
        Assert.Throws<InfiniteDecimalException>(() => { new BigDec(-0.5m).Ln(); });
        Assert.Throws<InfiniteDecimalException>(() => { new BigDec(-1).Ln(); });
        Assert.Throws<InfiniteDecimalException>(() => { new BigDec(-2).Ln(); });
    }
}