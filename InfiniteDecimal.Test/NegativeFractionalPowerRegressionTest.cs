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
        var expectedMantissa = BigInteger.Parse(
            "716350555406155007848817371546", CultureInfo.InvariantCulture);

        Assert.Equal(expectedMantissa, actual.Mantissa);
        Assert.Equal(30, actual.Offset);
        Assert.Equal(30, actual.MaxPrecision);
    }
}
