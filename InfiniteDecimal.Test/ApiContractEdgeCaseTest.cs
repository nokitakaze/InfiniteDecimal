using System.Globalization;
using System.Numerics;

namespace InfiniteDecimal.Test;

public class ApiContractEdgeCaseTest
{
    [Fact]
    public void ToType_SupportsBooleanConversion()
    {
        var zero = BigDec.Zero.ToType(typeof(bool), CultureInfo.InvariantCulture);
        var positive = BigDec.Parse("0.0001").ToType(typeof(bool), CultureInfo.InvariantCulture);
        var negative = BigDec.Parse("-0.0001").ToType(typeof(bool), CultureInfo.InvariantCulture);

        Assert.False(Assert.IsType<bool>(zero));
        Assert.True(Assert.IsType<bool>(positive));
        Assert.True(Assert.IsType<bool>(negative));
    }

    [Fact]
    public void ToString_UsesNumberFormatFromAnyFormatProvider()
    {
        var numberFormat = (NumberFormatInfo)CultureInfo.InvariantCulture.NumberFormat.Clone();
        numberFormat.NumberDecimalSeparator = "~";
        var value = new BigDec(123.45m);

        var direct = value.ToString(numberFormat);
        var throughConvertible = value.ToType(typeof(string), numberFormat);

        Assert.Equal("123~45", direct);
        Assert.Equal("123~45", throughConvertible);
    }

    [Fact]
    public void Parse_Null_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => BigDec.Parse(null!));
    }

    [Fact]
    public void ZeroReciprocalOperations_UseLibraryDivisionByZeroException()
    {
        var exceptions = new[]
        {
            Record.Exception(() => { _ = BigDec.Zero.Inverse(); }),
            Record.Exception(() => { _ = BigDec.Zero.Pow(BigInteger.MinusOne); }),
            Record.Exception(() => { _ = BigDec.Zero.Pow(-2); }),
        };

        Assert.All(exceptions, exception => Assert.IsType<DivideByZeroException>(exception));
    }

    [Fact]
    public void ReduceTrailingZeroesWithoutPower_ZeroHasIdentityOffsetPower()
    {
        var mantissa = BigInteger.Zero;
        var offset = 100;

        BigDec.ReduceTrailingZeroesWOPower(ref mantissa, ref offset, out var offsetPower);

        Assert.Equal(BigInteger.Zero, mantissa);
        Assert.Equal(0, offset);
        Assert.Equal(BigInteger.One, offsetPower);
    }

    [Fact]
    public void NegativePrecision_IsRejectedByEveryPublicEntryPoint()
    {
        var exceptions = new[]
        {
            Record.Exception(() => BigDec.AssertPrecision(-1)),
            Record.Exception(() => { _ = new BigDec(BigInteger.One, maxPrecision: -1); }),
            Record.Exception(() => { _ = new BigDec(BigInteger.One, offset: 0, maxPrecision: -1); }),
            Record.Exception(() => { _ = new BigDec(1m, maxPrecision: -1); }),
            Record.Exception(() => { _ = new BigDec(1d, maxPrecision: -1); }),
            Record.Exception(() => { _ = new BigDec(1f, maxPrecision: -1); }),
            Record.Exception(() => { _ = BigDec.One.WithPrecision(-1); }),
        };

        Assert.All(exceptions, exception => Assert.IsType<InfiniteDecimalException>(exception));
    }

    [Fact]
    public void PrecisionAboveImplementationLimit_ThrowsOutOfMemoryException()
    {
        Assert.Throws<OutOfMemoryException>(() => BigDec.AssertPrecision(646_456_994));
    }

    [Fact]
    public void Deconstruct_ReturnsCanonicalPublicFields()
    {
        var value = new BigDec(new BigInteger(1_234_500), offset: 4, maxPrecision: 50);

        var (mantissa, offset, offsetPower, maxPrecision) = value;

        Assert.Equal(value.Mantissa, mantissa);
        Assert.Equal(value.Offset, offset);
        Assert.Equal(BigDec.Pow10BigInt(offset), offsetPower);
        Assert.Equal(value.MaxPrecision, maxPrecision);
    }

    [Fact]
    public void EqualityAndHashCode_IgnorePrecisionForSameNumericRepresentation()
    {
        var first = new BigDec(123.45m, maxPrecision: 18);
        var second = first.WithPrecision(100);

        Assert.Equal(first, second);
        Assert.True(first == second);
        Assert.Equal(first.GetHashCode(), second.GetHashCode());
    }

    [Fact]
    public void Parse_RemovesDocumentedSpacesAndDigitSeparators()
    {
        var actual = BigDec.Parse(" 1_234.5_00 ");

        Assert.Equal(new BigDec(1234.5m), actual);
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(-1, 10)]
    [InlineData(-3, 1000)]
    public void PowFractionOfTen_NonPositivePowerReturnsInteger(int power, int expected)
    {
        var actual = BigDec.PowFractionOfTen(power);

        Assert.True(actual.IsInteger);
        Assert.Equal(new BigInteger(expected), actual.Mantissa);
    }
}
