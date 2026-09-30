using System.Globalization;
using System.Numerics;

namespace InfiniteDecimal.Test;

public class ParsingFormattingRegressionTest
{
    [Theory]
    [InlineData("--1")]
    [InlineData("-+1")]
    [InlineData("--1.25")]
    [InlineData("-+1.25")]
    [InlineData(".+1")]
    [InlineData(".-1")]
    [InlineData("++1")]
    [InlineData("1e+")]
    [InlineData("1e-")]
    [InlineData("")]
    [InlineData("NaN")]
    [InlineData("Infinity")]
    public void Parse_RejectsMalformedSignsAndMissingDigits(string text)
    {
        Assert.Throws<FormatException>(() => BigDec.Parse(text));
    }

    [Theory]
    [InlineData("+1.25", 125, 2)]
    [InlineData("-1.25", -125, 2)]
    [InlineData("+1.25E-3", 125, 5)]
    [InlineData("-1.25E+3", -1250, 0)]
    public void Parse_SingleLeadingSignRemainsValid(string text, int mantissa, int offset)
    {
        var actual = BigDec.Parse(text);
        Assert.Equal(new BigInteger(mantissa), actual.Mantissa);
        Assert.Equal(offset, actual.Offset);
    }

    [Theory]
    [InlineData("-12", "minus12")]
    [InlineData("-0.125", "minus0~125")]
    [InlineData("12.5", "12~5")]
    [InlineData("0", "0")]
    public void Formatting_HonorsProviderSignAndSeparatorForIntegersAndFractions(string text, string expected)
    {
        var value = BigDec.Parse(text);
        var format = (NumberFormatInfo)CultureInfo.InvariantCulture.NumberFormat.Clone();
        format.NegativeSign = "minus";
        format.NumberDecimalSeparator = "~";
        var culture = (CultureInfo)CultureInfo.InvariantCulture.Clone();
        culture.NumberFormat = format;

        // The wrapper exercises IFormatProvider.GetFormat, rather than concrete-type checks.
        foreach (IFormatProvider provider in new IFormatProvider[] { format, culture, new FormatProvider(format) })
        {
            Assert.Equal(expected, value.ToString(provider));
            Assert.Equal(expected, ((IConvertible)value).ToString(provider));
            Assert.Equal(expected, value.ToType(typeof(string), provider));
            Assert.Equal(expected, Convert.ChangeType(value, typeof(string), provider));
        }
    }

    [Theory]
    [InlineData("-12")]
    [InlineData("-0.125")]
    [InlineData("12.5")]
    public void DefaultFormatting_MakesVariantUnderCustomCurrentCulture(string text)
    {
        var value = BigDec.Parse(text);
        var originalCulture = CultureInfo.CurrentCulture;
        var customCulture = (CultureInfo)CultureInfo.InvariantCulture.Clone();
        customCulture.NumberFormat.NegativeSign = "minus";
        customCulture.NumberFormat.NumberDecimalSeparator = "~";
        try
        {
            CultureInfo.CurrentCulture = customCulture;
            Assert.Equal(text, value.ToString(CultureInfo.InvariantCulture));
            Assert.NotEqual(text, value.ToString(null));
            Assert.NotEqual(text, value.ToString(CultureInfo.CurrentCulture));
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
        }
    }

    [Theory]
    [InlineData("-12", "minus12")]
    [InlineData("-0.125", "minus0~125")]
    [InlineData("12.5", "12~5")]
    public void DefaultFormatting_InheritsCurrentCulture(string input, string expected)
    {
        var value = BigDec.Parse(input);
        var originalCulture = CultureInfo.CurrentCulture;
        var customCulture = (CultureInfo)CultureInfo.InvariantCulture.Clone();
        customCulture.NumberFormat.NegativeSign = "minus";
        customCulture.NumberFormat.NumberDecimalSeparator = "~";
        try
        {
            CultureInfo.CurrentCulture = customCulture;
            Assert.Equal(expected, value.ToString(null));
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
        }
    }

    private sealed class FormatProvider(NumberFormatInfo format) : IFormatProvider
    {
        public object? GetFormat(Type? formatType) => formatType == typeof(NumberFormatInfo) ? format : null;
    }
}
