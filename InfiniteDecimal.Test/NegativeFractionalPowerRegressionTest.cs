using System.Globalization;
using System.Numerics;

namespace InfiniteDecimal.Test;

public class NegativeFractionalPowerRegressionTest
{
    [Fact]
    public void NegativeFractionalPower_PreservesHigherExponentPrecision()
    {
        var value = new BigDec(1.1m, maxPrecision: 1);
        var exponent = new BigDec(-3.5m, maxPrecision: 30);

        var actual = value.Pow(exponent);

        // 1.1^(-3.5) = 1 / (1.331 * sqrt(1.1)).
        // Rounding once to 30 decimal places gives the coefficient below.
        // Rounding 1.331 to 1.33 before inversion must not change the result.
        var expectedMantissa = BigInteger.Parse("716350555406155007848817371546", CultureInfo.InvariantCulture);

        Assert.Equal(expectedMantissa, actual.Mantissa);
        Assert.Equal(30, actual.Offset);
        Assert.Equal(30, actual.MaxPrecision);
    }

    [Fact]
    public void NegativeFractionalPower_BaseBelowOne_DoesNotRoundToZeroBeforeInversion()
    {
        var value = new BigDec(0.25m, maxPrecision: 10);
        var exponent = new BigDec(-40.5m, maxPrecision: 10);

        var actual = value.Pow(exponent);

        // (1/4)^(-40.5) = 4^40 * 2 = 2^81, exactly.
        // The positive intermediate power must not round to zero before inversion.
        var expectedMantissa = BigInteger.One << 81;

        Assert.Equal(expectedMantissa, actual.Mantissa);
        Assert.Equal(0, actual.Offset);
        Assert.Equal(10, actual.MaxPrecision);
    }

    [Fact]
    public void NegativeFractionalPower_FractionalFactor_DoesNotRoundToZeroBeforeInversion()
    {
        var value = new BigDec(0.01m, maxPrecision: 10);
        var exponent = new BigDec(-10.5m, maxPrecision: 10);

        var actual = value.Pow(exponent);

        // (10^-2)^(-10.5) = 10^21, exactly.
        // 0.01^10 = 10^-20 survives at precision 20, but multiplying by
        // sqrt(0.01) = 0.1 must not round the intermediate 10^-21 to zero.
        var expectedMantissa = BigInteger.Pow(10, 21);

        Assert.Equal(expectedMantissa, actual.Mantissa);
        Assert.Equal(0, actual.Offset);
        Assert.InRange(actual.MaxPrecision, 10, 20);
    }

    [Theory]
    [InlineData(10, 10, "316227766016837933200")]
    [InlineData(20, 10, "3162277660168379331998893544433")]
    [InlineData(20, 30, "316227766016837933199889354443271853371955513932522")]
    public void NegativeFractionalPower_InversionPreservesRequestedDecimalPlaces(
        int integerPart, int precision, string expectedCoefficient)
    {
        var value = new BigDec(0.1m, maxPrecision: precision);
        var exponent = new BigDec(-integerPart - 0.5m, maxPrecision: precision);

        var actual = value.Pow(exponent);

        // 0.1^(-(n + 0.5)) = 10^n * sqrt(10).
        // Coefficients are independently rounded once to 'precision' decimal places.
        // Keeping the intermediate nonzero is insufficient: inversion amplifies
        // its rounding error, which can corrupt even the integer part of the result.
        var expected = new BigDec(
            BigInteger.Parse(expectedCoefficient, CultureInfo.InvariantCulture),
            precision,
            precision
        );

        Assert.Equal(expected, actual);
        Assert.Equal(precision, actual.MaxPrecision);
    }
}
