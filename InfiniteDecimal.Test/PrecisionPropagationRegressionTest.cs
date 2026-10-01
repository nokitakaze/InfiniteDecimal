using System.Numerics;

namespace InfiniteDecimal.Test;

public class PrecisionPropagationRegressionTest
{
    [Theory]
    [InlineData(5, "Int32", 4)]
    [InlineData(5, "Int64", 4)]
    [InlineData(5, "BigInteger", 4)]
    [InlineData(1, "Int32", 100)]
    [InlineData(1, "Int64", 100)]
    [InlineData(1, "BigInteger", 100)]
    public void NegativePower_DoesNotRoundPositivePowerBeforeTakingReciprocal(
        int mantissa, string overload, int expected)
    {
        var value = new BigDec(new BigInteger(mantissa), offset: 1, maxPrecision: 1);

        var actual = overload switch
        {
            "Int32" => value.Pow(-2),
            "Int64" => value.Pow(-2L),
            "BigInteger" => value.Pow(new BigInteger(-2)),
            _ => throw new ArgumentOutOfRangeException(nameof(overload)),
        };

        // 0.5^-2 = 4 and 0.1^-2 = 100. Rounding 0.25 or 0.01 to
        // one decimal place before inversion changes the problem being solved.
        Assert.Equal(new BigInteger(expected), actual.Mantissa);
        Assert.Equal(0, actual.Offset);
    }

    [Fact]
    public void ExpWithBigPrecision_Twenty_PreservesTwoDecimalPlaces()
    {
        var value = new BigDec(new BigInteger(20), maxPrecision: 2);

        var actual = value.ExpWithBigPrecision();

        // exp(20) = 485165195.4097902779691068305415...
        // Reference rounded independently of BigDec.Exp and the stored E constant.
        Assert.Equal(new BigInteger(48_516_519_541L), actual.Mantissa);
        Assert.Equal(2, actual.Offset);
    }

    [Theory]
    [InlineData(-199_999, 5)]
    [InlineData(-1_999_999, 6)]
    [InlineData(-200_000, 5)]
    [InlineData(-4, 2)]
    [InlineData(199_999, 5)]
    public void Inverse_SignedInteger_UsesEnoughPrecisionForTheEntireRemainder(
        int denominator, int precision)
    {
        // Integer inputs keep Offset zero. The negative cases exercise the sign
        // handling in the precision estimate without a large fractional scale.
        var value = new BigDec(new BigInteger(denominator), maxPrecision: precision);

        var actual = value.Inverse();

        AssertRoundedRational(actual, BigInteger.One, new BigInteger(denominator), precision);
    }

    [Theory]
    [InlineData(101, 2, 2, 1000)]
    [InlineData(15, 1, 1, 100)]
    [InlineData(12345, 4, 4, 100)]
    [InlineData(10001, 4, 4, 1000)]
    [InlineData(9999, 4, 4, 1000)]
    [InlineData(27, 2, 2, 4)]
    [InlineData(5, 1, 1, 4)]
    [InlineData(-5, 1, 1, 3)]
    [InlineData(-101, 2, 2, 31)]
    [InlineData(101, 2, 6, 32)]
    [InlineData(101, 2, 6, 33)]
    public void PositiveIntegerPower_MatchesExactRationalReference(
        int mantissa, int offset, int precision, int exponent)
    {
        // All inputs are exactly representable at the requested precision.
        // BigInteger powers form an exact oracle; BigDec.Pow is only used for actual.
        var value = new BigDec(new BigInteger(mantissa), offset, precision);
        var numerator = BigInteger.Pow(new BigInteger(mantissa), exponent);
        var denominator = BigInteger.Pow(10, checked(offset * exponent));

        var actual = value.Pow(exponent);

        AssertRoundedRational(actual, numerator, denominator, precision);
    }

    private static void AssertRoundedRational(
        BigDec actual, BigInteger numerator, BigInteger denominator, int precision)
    {
        var sign = numerator.Sign * denominator.Sign;
        var divisor = BigInteger.Abs(denominator);
        var scaledNumerator = BigInteger.Abs(numerator) * BigInteger.Pow(10, precision);
        var expected = BigInteger.DivRem(scaledNumerator, divisor, out var remainder);
        if (2 * remainder > divisor || (2 * remainder == divisor && !expected.IsEven))
            expected++;
        expected *= sign;

        // Compare exact scaled integers; do not use BigDec rounding, conversion,
        // parsing or arithmetic to calculate the expected result.
        Assert.InRange(actual.Offset, 0, precision);
        var actualScaled = actual.Mantissa * BigInteger.Pow(10, precision - actual.Offset);
        Assert.Equal(expected, actualScaled);
    }
}
