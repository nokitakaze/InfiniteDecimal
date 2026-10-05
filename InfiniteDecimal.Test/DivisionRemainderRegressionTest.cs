using System.Numerics;

namespace InfiniteDecimal.Test;

public class DivisionRemainderRegressionTest
{
    public static IEnumerable<object[]> JustBeyondMidpointCases()
    {
        foreach (var precision in new[] { 1, 2, 10 })
        foreach (var numeratorSign in new[] { -1, 1 })
        foreach (var denominatorSign in new[] { -1, 1 })
            yield return new object[] { precision, numeratorSign, denominatorSign };
    }

    [Theory]
    [MemberData(nameof(JustBeyondMidpointCases))]
    public void Division_JustBeyondMidpoint_PreservesRemainderForFinalRounding(
        int precision, int numeratorSign, int denominatorSign)
    {
        var numeratorMagnitude = 2 * BigInteger.Pow(10, 9 * precision) + BigInteger.One;
        var denominatorMagnitude = 4 * BigInteger.Pow(10, 10 * precision);
        var numerator = new BigDec(numeratorSign * numeratorMagnitude, maxPrecision: precision);
        var denominator = new BigDec(denominatorSign * denominatorMagnitude, maxPrecision: precision);

        var actual = numerator / denominator;

        // The exact magnitude is 0.5 * 10^-precision + 1 / denominatorMagnitude.
        // It lies strictly above the midpoint, so the rounded magnitude is 10^-precision.
        // For precision 1: 2000000001 / 40000000000 = 0.050000000025 -> 0.1.
        // Discarding the division remainder must not turn this into the exact tie 0.05.
        Assert.Equal(new BigInteger(numeratorSign * denominatorSign), actual.Mantissa);
        Assert.Equal(precision, actual.Offset);
    }

    public static IEnumerable<object[]> ZeroPrecisionJustBeyondMidpointCases()
    {
        var fractions = new (int numerator, int denominator)[]
        {
            (4, 7),
            (5, 9),
            (50, 99),
            (500, 999),
        };

        foreach (var (numerator, denominator) in fractions)
        foreach (var numeratorSign in new[] { -1, 1 })
        foreach (var denominatorSign in new[] { -1, 1 })
            yield return new object[]
            {
                numeratorSign * numerator,
                denominatorSign * denominator,
                numeratorSign * denominatorSign,
            };
    }

    [Theory]
    [MemberData(nameof(ZeroPrecisionJustBeyondMidpointCases))]
    public void Division_ZeroPrecision_JustBeyondMidpointDoesNotBecomeAnExactTie(
        int numerator, int denominator, int expected)
    {
        var left = new BigDec(new BigInteger(numerator), maxPrecision: 0);
        var right = new BigDec(new BigInteger(denominator), maxPrecision: 0);

        var actual = left / right;

        // Each exact magnitude lies strictly between 0.5 and 1, so the nearest
        // integer has magnitude 1. For example, 5/9 = 0.555... must round to 1.
        // Truncating the intermediate to 0.5 incorrectly changes this into a tie.
        Assert.Equal(new BigInteger(expected), actual.Mantissa);
        Assert.Equal(0, actual.Offset);
    }
}
