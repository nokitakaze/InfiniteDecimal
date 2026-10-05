using System.Numerics;

namespace InfiniteDecimal.Test;

public class NegativeExpRoundingRegressionTest
{
    public static IEnumerable<object[]> NegativeExponentCases()
    {
        var cases = new (int coefficient, int precision, int expectedCoefficient)[]
        {
            (3, 1, 7),
            (6, 1, 5),
            (8, 1, 4),
            (8, 2, 92),
            (11, 2, 90),
            (125, 3, 882),
        };

        foreach (var (coefficient, precision, expectedCoefficient) in cases)
        foreach (var useBigPrecision in new[] { false, true })
            yield return new object[] { coefficient, precision, expectedCoefficient, useBigPrecision };
    }

    [Theory]
    [MemberData(nameof(NegativeExponentCases))]
    public void NegativeExp_DoesNotRoundPositiveExponentialBeforeInversion(
        int coefficient, int precision, int expectedCoefficient, bool useBigPrecision)
    {
        var value = new BigDec(new BigInteger(-coefficient), precision, precision);

        var actual = useBigPrecision ? value.ExpWithBigPrecision() : value.Exp();

        // References were bounded independently by consecutive alternating Taylor sums.
        // exp(-0.3) = 0.740818... -> 0.7; rounding exp(0.3) to 1.3 first gives 0.8.
        // exp(-0.125) = 0.882496... -> 0.882, not 0.883.
        Assert.InRange(actual.Offset, 0, precision);
        var actualCoefficient = actual.Mantissa * BigInteger.Pow(10, precision - actual.Offset);
        Assert.Equal(new BigInteger(expectedCoefficient), actualCoefficient);
    }
}
