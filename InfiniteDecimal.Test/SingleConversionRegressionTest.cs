using System.Globalization;

namespace InfiniteDecimal.Test;

public class SingleConversionRegressionTest
{
    public static IEnumerable<object[]> MidpointCases()
    {
        // Exact midpoints between consecutive Single values near 1, and values
        // just to either side. A conversion through Double loses the last digit.
        var cases = new (string text, int expectedBits)[]
        {
            ("1.000000059604644775390625", 0x3f800000),
            ("1.0000000596046447753906249999999999", 0x3f800000),
            ("1.0000000596046447753906250000000001", 0x3f800001),
            ("1.000000178813934326171875", 0x3f800002),
            ("1.0000001788139343261718749999999999", 0x3f800001),
            ("1.0000001788139343261718750000000001", 0x3f800002),
        };
        foreach (var (text, bits) in cases)
        foreach (var negative in new[] { false, true })
        foreach (var route in new[] { "Cast", "Interface", "ToType", "Convert", "ChangeType" })
            yield return new object[]
            {
                negative ? "-" + text : text,
                negative ? bits | int.MinValue : bits,
                route,
            };
    }

    [Theory]
    [MemberData(nameof(MidpointCases))]
    public void SingleConversion_RoundsDirectlyToTheNearestSingle(string text, int expectedBits, string route)
    {
        var value = BigDec.Parse(text);
        var provider = CultureInfo.InvariantCulture;
        var actual = route switch
        {
            "Cast" => (float)value,
            "Interface" => ((IConvertible)value).ToSingle(provider),
            "ToType" => (float)value.ToType(typeof(float), provider),
            "Convert" => Convert.ToSingle((object)value, provider),
            "ChangeType" => (float)Convert.ChangeType(value, typeof(float), provider),
            _ => throw new ArgumentOutOfRangeException(nameof(route)),
        };
        Assert.Equal(expectedBits, BitConverter.SingleToInt32Bits(actual));
    }
}
