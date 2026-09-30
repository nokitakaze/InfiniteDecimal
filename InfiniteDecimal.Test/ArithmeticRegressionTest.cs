using System.Numerics;

namespace InfiniteDecimal.Test;

public class ArithmeticRegressionTest
{
    public static IEnumerable<object[]> ReciprocalCases()
    {
        // Includes exact ties, non-ties with a first discarded digit of 5,
        // and results whose retained mantissa is zero before rounding.
        var cases = new (int denominator, int precision, int mantissa, int offset)[]
        {
            (15, 1, 1, 1),
            (18, 1, 1, 1),
            (19, 1, 1, 1),
            (20, 1, 0, 0),
            (21, 1, 0, 0),
            (150, 2, 1, 2),
            (180, 2, 1, 2),
            (200, 2, 0, 0),
            (40, 2, 2, 2),
            (8, 2, 12, 2),
            (7, 0, 0, 0),
        };
        foreach (var (denominator, precision, mantissa, offset) in cases)
        foreach (var sign in new[] { -1, 1 })
        foreach (var operation in new[] { "Inverse", "Divide", "PowInt", "PowBigInteger", "PowBigDec" })
            yield return new object[] { sign * denominator, precision, sign * mantissa, offset, operation };
    }

    [Theory]
    [MemberData(nameof(ReciprocalCases))]
    public void Reciprocal_RoundsUsingSignAndEntireRemainder(
        int denominator, int precision, int expectedMantissa, int expectedOffset, string operation)
    {
        var value = new BigDec(new BigInteger(denominator), maxPrecision: precision);
        var actual = operation switch
        {
            "Inverse" => value.Inverse(),
            "Divide" => new BigDec(BigInteger.One, precision) / value,
            "PowInt" => value.Pow(-1),
            "PowBigInteger" => value.Pow(BigInteger.MinusOne),
            "PowBigDec" => value.Pow(new BigDec(BigInteger.MinusOne, precision)),
            _ => throw new ArgumentOutOfRangeException(nameof(operation)),
        };

        Assert.Equal(new BigInteger(expectedMantissa), actual.Mantissa);
        Assert.Equal(expectedOffset, actual.Offset);
    }

    [Theory]
    [InlineData(3, 2, 1)]
    [InlineData(5, 2, 2)]
    [InlineData(7, 2, 3)]
    [InlineData(-3, 2, -1)]
    [InlineData(3, -2, -1)]
    [InlineData(-3, -2, 1)]
    [InlineData(2, 3, 0)]
    [InlineData(-2, 3, 0)]
    [InlineData(2, -3, 0)]
    public void Division_ZeroPrecisionStillRoundsToNearestEven(int numerator, int denominator, int expected)
    {
        // Both operands must have precision zero; a primitive operand would introduce precision 18.
        var actual = new BigDec(new BigInteger(numerator), 0) / new BigDec(new BigInteger(denominator), 0);
        Assert.Equal(new BigInteger(expected), actual.Mantissa);
        Assert.Equal(0, actual.Offset);
    }

    [Theory]
    [InlineData(0, "0.5")]
    [InlineData(0, "0.500000000000000000000000000001")]
    [InlineData(0, "0.499999999999999999999999999999")]
    [InlineData(2, "1.245")]
    [InlineData(2, "1.245000000000000000000000000001")]
    [InlineData(2, "1.244999999999999999999999999999")]
    public void Round_UsesAllDigitsOnBothSidesOfMidpoint(int precision, string text)
    {
        foreach (var sign in new[] { -1, 1 })
        {
            var value = BigDec.Parse(text);
            if (sign < 0) value = -value;
            var divisor = BigInteger.Pow(10, value.Offset - precision);
            var quotient = BigInteger.DivRem(BigInteger.Abs(value.Mantissa), divisor, out var remainder);
            if (2 * remainder > divisor || (2 * remainder == divisor && !quotient.IsEven))
                quotient++;
            // Compare scaled integers, independently of BigDec constructors and equality.
            var actual = value.Round(precision);
            Assert.InRange(actual.Offset, 0, precision);
            Assert.Equal(sign * quotient, actual.Mantissa * BigInteger.Pow(10, precision - actual.Offset));
        }
    }

    [Theory]
    [InlineData(10000100000L, 100_000)]
    [InlineData(10000100001L, 100_001)]
    [InlineData(10000000100000000L, 100_000_000)]
    [InlineData(10000000100000001L, 100_000_001)]
    public void Sqrt_RoundsNearMidpointWithoutLosingTheRemainder(BigInteger input, int expected)
    {
        // (100000.5)^2 = 10000100000.25. The inputs straddle this exact midpoint.
        var actual = new BigDec(input, 0).Sqrt();
        Assert.Equal(new BigInteger(expected), actual.Mantissa);
        Assert.Equal(0, actual.Offset);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(8)]
    [InlineData(32)]
    [InlineData(128)]
    public void IntegerSqrt_IsCorrectImmediatelyAroundLargePerfectSquares(int bits)
    {
        var root = (BigInteger.One << bits) + 1;
        var square = root * root;
        Assert.Equal(root - 1, BigDec.Sqrt(square - 1));
        Assert.Equal(root, BigDec.Sqrt(square));
        Assert.Equal(root, BigDec.Sqrt(square + 1));
    }

    [Theory]
    [InlineData(5, 1, 1, 1)]
    [InlineData(-5, 1, 1, 1)]
    [InlineData(27, 2, 1, 2)]
    [InlineData(-27, 2, 1, 2)]
    public void IntegerPower_DoesNotRoundIntermediateSquaresTooEarly(
        int mantissa, int precision, int expectedMantissa, int expectedOffset)
    {
        // 0.5^4 = 0.0625 -> 0.1; 0.27^4 = 0.00531441 -> 0.01.
        var value = new BigDec(new BigInteger(mantissa), precision, precision);
        var actual = value.Pow(4);
        Assert.Equal(new BigInteger(expectedMantissa), actual.Mantissa);
        Assert.Equal(expectedOffset, actual.Offset);
    }

    [Theory]
    [InlineData("2147483648", 1)]
    [InlineData("2147483649", -1)]
    [InlineData("-2147483648", 1)]
    [InlineData("-2147483649", -1)]
    public void DecimalPower_IntegralExponentIsNotLimitedToInt32(string exponent, int expected)
    {
        var value = new BigDec(-1);
        var power = BigDec.Parse(exponent);
        Assert.Equal(new BigInteger(expected), value.Pow(power).Mantissa);
        Assert.Equal(new BigInteger(expected), value.Pow(BigInteger.Parse(exponent)).Mantissa);
    }
}
