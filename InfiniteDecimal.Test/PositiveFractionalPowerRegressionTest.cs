using System.Globalization;
using System.Numerics;

namespace InfiniteDecimal.Test;

public class PositiveFractionalPowerRegressionTest
{
    [Theory]
    [InlineData(10, 10, "316227766016837933200")]
    [InlineData(20, 10, "3162277660168379331998893544433")]
    [InlineData(20, 30, "316227766016837933199889354443271853371955513932522")]
    public void PositiveFractionalPower_LargeResult_PreservesRequestedDecimalPlaces(
        int integerPart, int precision, string expectedCoefficient)
    {
        var value = new BigDec(10m, maxPrecision: precision);
        var exponent = new BigDec(integerPart + 0.5m, maxPrecision: precision);

        var actual = value.Pow(exponent);

        // 10^(n + 0.5) = 10^n * sqrt(10).
        // Coefficients are independently rounded once to 'precision' decimal places.
        // Rounding sqrt(10) before multiplication by 10^n must not lose the
        // requested decimal places or corrupt the integer part of the result.
        var expected = new BigDec(
            BigInteger.Parse(expectedCoefficient, CultureInfo.InvariantCulture),
            precision,
            precision
        );

        Assert.Equal(expected, actual);
        Assert.Equal(precision, actual.MaxPrecision);
    }
}
