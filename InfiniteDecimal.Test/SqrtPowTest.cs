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
            (decimal)Math.PI,
            (decimal)Math.Pow(Math.E, 2),
            (decimal)Math.Pow(Math.E, 1.5d),
            (decimal)Math.Sqrt(Math.E),
            1.337m,
            15m,
            1234m,
            1.000_000_01m,
            0.000_000_01m,
            911,
            228,
        };

        var xModifiers = new decimal[] { 0.5m, 1m, 1.5m, 2m, 3m, };
        var pModifier = new decimal[] { 1m, 0.1m, 0.001m, 0.000_01m, 0.000_000_1m, };
        var modifiedE = xModifiers
            .Select(x => (decimal)Math.E * x)
            .SelectMany(v => pModifier.SelectMany(p => new decimal[] { v + p, v - p, v + 2 * p, v - 2 * p, }))
            .SelectMany(v => new decimal[] { v, 1m / v })
            .ToArray();

        return values
            .Concat(modifiedE)
            .Concat(values.Select(t => t - 1))
            .Concat(values.Select(t => t + 1))
            .Distinct()
            .Where(x => x >= 0)
            .OrderBy(t => t)
            .ToArray();
    }

    #region Ln & Sqrt

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

        var actual1 = powActual.Sqrt();
        if (TrivialMathTests.IsInt(input))
        {
            Assert.True(powExpected == powActual);
            Assert.Equal(new BigDec(input), actual1);
        }
        else
        {
            var diff = (powExpected - powActual).Abs();
            Assert.True(diff < 0.000_000_1m);

            diff = (actual1 - input).Abs();
            Assert.True(diff < 0.000_000_1m);
        }

        {
            var actual2a = powActual.Pow(0.5m);
            var diff = (actual2a - input).Abs();
            Assert.True(diff < 0.000_000_1m);

            var actual2b = powActual.Pow(0.5d);
            diff = (actual2b - input).Abs();
            Assert.True(diff < 0.000_000_1m);
        }
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
        var actual = new BigDec(input).Ln().WithPrecision(20);

        var diff = (actual - expected).Abs();
        Assert.True(diff < 0.000_000_1m);

        var actualExp = actual.Exp();
        diff = (actualExp - input).Abs();
        Assert.True(diff < 0.000_000_1m);
    }

    [Fact]
    public void TestLn_Euler()
    {
        var expected = BigDec.One;
        var actual = new BigDec(BigDec.E).Ln().WithPrecision(20);

        var diff = (actual - expected).Abs();
        var epsilon = BigDec.Parse("0.00000000000000000001");
        Assert.True(diff < epsilon);

        var actualExp = actual.Exp();
        diff = (actualExp - BigDec.E).Abs();
        Assert.True(diff < 0.000_000_1m);
    }

    #endregion

    #region Exceptions

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

    #endregion

    #region Power

    public static object[][] TestPowData()
    {
        var values = GetTestDecimal();
        decimal[] exps = Enumerable
            .Range(0, 11)
            .Select(t => t * 0.5m)
            .Concat(new[] { 1488m, 0.000001m, 1_000_000m, })
            .SelectMany(t => new decimal[]
                { t, t + 0.000001m, t + 0.001m, t - 0.000001m, t - 0.001m, t + 0.007m, t + 0.001_002_003m })
            .ToArray();
        exps = exps
            .Concat(exps.Select(exp => -exp))
            .Distinct()
            .OrderBy(t => t)
            .ToArray();

        var rnd = new Random();
        return values
            .OrderBy(t => t)
            .Distinct()
            .SelectMany(value => exps.Select(exp => new object[] { value, exp }))
            .OrderBy(_ => rnd.NextDouble())
            .Take(100) // todo delme
            .ToArray();
    }

    [Theory]
    [MemberData(nameof(TestPowData))]
    public void TestPow(decimal value, decimal exponent)
    {
        var expected = Math.Pow((double)value, (double)exponent);
        if (!double.IsFinite(expected))
        {
            return;
        }

        var valueBI = new BigDec(value);
        var exponentBI = new BigDec(exponent);

        // ReSharper disable once ConvertIfStatementToSwitchStatement
        if ((expected == 0) && (value != 0))
        {
            // Too small value for 64-bit IEEE-754
        }
        else if (expected == 0)
        {
            var actual1 = valueBI.Pow(exponentBI);
            Assert.Equal(BigDec.Zero, actual1);

            var actual2 = valueBI.Pow(exponent);
            Assert.Equal(BigDec.Zero, actual2);
        }
        else if (expected == 1)
        {
            var actual1 = valueBI.Pow(exponentBI);
            Assert.Equal(BigDec.One, actual1);

            var actual2 = valueBI.Pow(exponent);
            Assert.Equal(BigDec.One, actual2);
        }
        else
        {
            var actual1 = valueBI.Pow(exponentBI);
            // var diff = (expected - actual1).Abs();
            // Assert.True(diff < 0.000_000_1m);
            Assert.InRange(actual1, new BigDec(expected) - 0.000_000_1m, new BigDec(expected) + 0.000_000_1m);

            var actual2 = valueBI.Pow(exponent);
            // diff = (expected - actual2).Abs();
            // Assert.True(diff < 0.000_000_1m);
            Assert.InRange(actual2, new BigDec(expected) - 0.000_000_1m, new BigDec(expected) + 0.000_000_1m);
        }
    }

    [Fact]
    public void TestPow_Case1()
    {
        TestPow(2.71826182845904m, -1000000.007m);
    }

    [Fact]
    public void TestPow_Case2()
    {
        TestPow(14, 0.007m);
    }

    [Fact]
    public void TestPow_Case3()
    {
        TestPow(7.95484548537712m, 0m);
    }

    [Fact]
    public void TestPow_Zero_Zero()
    {
        var expected = BigDec.Zero.Pow(BigDec.Zero);
        Assert.Equal(BigDec.One, expected);
    }

    #endregion
}