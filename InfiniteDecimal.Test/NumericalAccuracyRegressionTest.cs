using System.Globalization;
using System.Numerics;

namespace InfiniteDecimal.Test;

public class NumericalAccuracyRegressionTest
{
    public static IEnumerable<object[]> Inverse_ValueJustAboveMidpoint_DoesNotRoundToZero_Data()
    {
        return Enumerable
            .Range(1, 10)
            .Select(i => new object[] { i * 2 });
    }

    [Theory]
    [MemberData(nameof(Inverse_ValueJustAboveMidpoint_DoesNotRoundToZero_Data))]
    public void Inverse_ValueJustAboveMidpoint_DoesNotRoundToZero(int power)
    {
        var i = BigInteger.Pow(new BigInteger(10), power) * 2 - BigInteger.One;
        var value = new BigDec(i, maxPrecision: power);

        var actual = value.Inverse();

        // 1 / 199999 = 0.000005000025000125... > 0.000005.
        // At five decimal places the nearest value is 0.00001, not zero.
        Assert.Equal(BigInteger.One, actual.Mantissa);
        Assert.Equal(power, actual.Offset);
    }

    public static IEnumerable<object[]> Sqrt_ValueJustAboveSquaredMidpoint_RoundsUp_Data()
    {
        return Enumerable
            .Range(1, 10)
            .Select(i => new object[] { i * 2 });
    }

    [Theory]
    [MemberData(nameof(Sqrt_ValueJustAboveSquaredMidpoint_RoundsUp_Data))]
    public void Sqrt_ValueJustAboveSquaredMidpoint_RoundsUp(int power)
    {
        var i1 = BigInteger.Pow(new BigInteger(10), power + 1) + 5;
        var input = i1 * i1 + 75;
        input /= 100;
        var value = new BigDec(input, maxPrecision: 0);
        var actual = value.Sqrt();

        // Let n = 10000000000. The input is n^2 + n + 1,
        // which is above (n + 0.5)^2 = n^2 + n + 0.25.
        // Truncating the root to ten guard places must not turn it into a tie.
        var expectedMantissa = BigInteger.Pow(new BigInteger(10), power) + 1;
        Assert.Equal(expectedMantissa, actual.Mantissa);
        Assert.Equal(0, actual.Offset);
    }

    [Fact]
    public void Pow_LargeIntegerExponent_PreservesRequestedDecimalPlaces()
    {
        var value = new BigDec(new BigInteger(101), offset: 2, maxPrecision: 2);

        var actual = value.Pow(1000);

        // (101 / 100)^1000 = 20959.155637813660064441245788...
        // Rounding the exact rational result to two decimal places gives 20959.16.
        Assert.Equal(new BigInteger(20_959_16), actual.Mantissa);
        Assert.Equal(2, actual.Offset);
    }

    [Fact]
    public void ExpWithBigPrecision_Ten_RoundsCorrectlyToEighteenDecimalPlaces()
    {
        var value = new BigDec(new BigInteger(10), maxPrecision: 18);

        var actual = value.ExpWithBigPrecision();

        // exp(10) = 22026.465794806716516957900645284244...
        // The expected coefficient is independent of BigDec.Exp and the stored E constant.
        var expectedMantissa = BigInteger.Parse("22026465794806716516958", CultureInfo.InvariantCulture);
        Assert.Equal(expectedMantissa, actual.Mantissa);
        Assert.Equal(18, actual.Offset);
    }
}
