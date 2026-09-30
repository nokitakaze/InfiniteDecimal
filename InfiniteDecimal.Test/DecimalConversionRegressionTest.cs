using System.Globalization;

namespace InfiniteDecimal.Test;

public class DecimalConversionRegressionTest
{
    public static IEnumerable<object[]> RoundingCases()
    {
        var cases = new (string input, string expected)[]
        {
            // Scale <= 28, but the coefficient still exceeds 96 bits.
            ("10000000000000000000000000000.6", "10000000000000000000000000001"),
            ("10000000000000000000000000000.5", "10000000000000000000000000000"),
            ("10000000000000000000000000001.5", "10000000000000000000000000002"),
            ("7.92281625142643375935439503356", "7.922816251426433759354395034"),
            ("7.9228162514264337593543950335", "7.9228162514264337593543950335"),
            ("10.0000000000000000000000000045", "10.000000000000000000000000004"),
            ("10.0000000000000000000000000055", "10.000000000000000000000000006"),
            // Avoid double rounding when both scale and coefficient must be reduced.
            ("10.00000000000000000000000000451", "10.000000000000000000000000005"),
            ("10.00000000000000000000000000549", "10.000000000000000000000000005"),
        };
        foreach (var (input, expected) in cases)
        foreach (var sign in new[] { "", "-" })
        foreach (var route in new[] { "Cast", "Interface", "ToType", "Convert", "ChangeType" })
            yield return new object[] { sign + input, sign + expected, route };
    }

    [Theory]
    [MemberData(nameof(RoundingCases))]
    public void DecimalConversion_RoundsOnceToARepresentable96BitCoefficient(
        string text, string expectedText, string route)
    {
        var provider = CultureInfo.InvariantCulture;
        var expected = decimal.Parse(expectedText, provider);
        Assert.Equal(expectedText, expected.ToString(provider));
        var value = BigDec.Parse(text);
        var actual = route switch
        {
            "Cast" => (decimal)value,
            "Interface" => ((IConvertible)value).ToDecimal(provider),
            "ToType" => (decimal)value.ToType(typeof(decimal), provider),
            // ReSharper disable once RedundantCast
            "Convert" => Convert.ToDecimal((object)value, provider),
            "ChangeType" => (decimal)Convert.ChangeType(value, typeof(decimal), provider),
            _ => throw new ArgumentOutOfRangeException(nameof(route)),
        };
        Assert.Equal(expected, actual);
    }
}
