using System.Globalization;
using System.Numerics;

namespace InfiniteDecimal.Test;

public class SqrtGuardDigitsRegressionTest
{
    [Theory]
    [InlineData(12, "635794335170", "797367126969")]
    [InlineData(18, "4434334293989764", "66590797367126969")]
    [InlineData(30, "65200405144023254281959078436", "255343700028066590797367126969")]
    public void Sqrt_JustAboveMidpoint_RoundsUpDespiteSmallResidual(
        int precision, string inputCoefficient, string expectedCoefficient)
    {
        var input = BigInteger.Parse(inputCoefficient, CultureInfo.InvariantCulture);
        var expected = BigInteger.Parse(expectedCoefficient, CultureInfo.InvariantCulture);

        // Certify the reference using integer comparisons, independently of BigDec.Sqrt.
        // The scaled root lies between expected-1 and expected, strictly above their midpoint.
        var scaledRadicand = input * BigInteger.Pow(10, precision);
        var lower = expected - 1;
        Assert.True(lower * lower < scaledRadicand);
        Assert.True(scaledRadicand < expected * expected);
        Assert.True(BigInteger.Pow(2 * lower + 1, 2) < 4 * scaledRadicand);

        var actual = new BigDec(input, precision, precision).Sqrt();

        Assert.Equal(expected, actual.Mantissa);
        Assert.Equal(precision, actual.Offset);
    }
}
