using System.Globalization;
using System.Numerics;

namespace InfiniteDecimal.Test;

public class CorrectnessContractTest
{
    [Theory]
    [InlineData(18, "0.166666666666666667")]
    [InlineData(5, "0.16667")]
    [InlineData(1, "0.2")]
    public void ReciprocalOfSix_RoundsHalfUp(int precision, string expectedText)
    {
        var six = new BigDec(new BigInteger(6), precision);
        var expected = BigDec.Parse(expectedText);

        Assert.Equal(expected, six.Inverse());
        Assert.Equal(expected, six.Pow(-1));
        Assert.Equal(expected, six.Pow(-1d));
        Assert.Equal(expected, six.Pow(BigInteger.MinusOne));
        Assert.Equal(expected, new BigDec(BigInteger.One, precision) / six);
        Assert.Equal(expected, new BigDec(new BigInteger(2), precision) / new BigDec(new BigInteger(12), precision));
    }

    [Fact]
    public void TwoThirds_RoundsAwayFromTheTruncatedExpansion()
    {
        var expected = BigDec.Parse("0.666666666666666667");
        var negative = BigDec.Parse("-0.666666666666666667");

        Assert.Equal(expected, new BigDec(2) / new BigDec(3));
        Assert.Equal(expected, new BigDec(2) / 3);
        Assert.Equal(negative, new BigDec(-2) / 3);
    }

    [Fact]
    public void ThreeSixteenths_RoundsTieToEven()
    {
        var quotient = new BigDec(new BigInteger(3), 3) / new BigDec(new BigInteger(16), 3);

        Assert.Equal(BigDec.Parse("0.188"), quotient);
    }

    [Fact]
    public void Exp_Zero_IsExactlyOne()
    {
        foreach (var precision in new[] { 1, 8, 18, 30 })
        {
            var actual = new BigDec(BigInteger.Zero, precision).Exp();
            Assert.Equal(BigInteger.One, actual.Mantissa);
            Assert.Equal(0, actual.Offset);
        }

        Assert.Equal(BigInteger.One, BigDec.Zero.Exp().Mantissa);
        Assert.Equal(0, BigDec.Zero.Exp().Offset);
    }

    [Fact]
    public void ToDecimal_RoundsInsteadOfTruncatingExtraDigits()
    {
        var roundedUp = BigDec.Parse("0.12345678901234567890123456786");
        const decimal expectedRoundedUp = 0.1234567890123456789012345679m;

        Assert.Equal(expectedRoundedUp, (decimal)roundedUp);
        Assert.Equal(expectedRoundedUp, roundedUp.ToDecimal(CultureInfo.InvariantCulture));
        Assert.Equal(expectedRoundedUp, roundedUp.ToType(typeof(decimal), CultureInfo.InvariantCulture));
        Assert.Equal(-expectedRoundedUp, (decimal)(-roundedUp));

        // https://learn.microsoft.com/en-us/dotnet/api/system.decimal.parse?view=net-10.0
        // "rounding to nearest"
        var halfway = BigDec.Parse("0.00000000000000000000000000015");
        const decimal expectedHalfway = 0.0000000000000000000000000002m;

        Assert.Equal(expectedHalfway, (decimal)halfway);
        Assert.Equal(-expectedHalfway, (decimal)(-halfway));
    }

    [Theory]
    [InlineData("1e10", "10000000000")]
    [InlineData("1E10", "10000000000")]
    [InlineData("1E+2", "100")]
    [InlineData("1.5E-1", "0.15")]
    [InlineData("2.5e2", "250")]
    [InlineData("-4E+1", "-40")]
    [InlineData("1e-1", "0.1")]
    public void Parse_AcceptsStandardScientificNotation(string input, string expectedText)
    {
        var expectedFromText = BigDec.Parse(expectedText);
        var actual = BigDec.Parse(input);
        Assert.Equal(expectedFromText, actual);
    }
}
