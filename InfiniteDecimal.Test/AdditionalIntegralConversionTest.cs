using System.Globalization;
using System.Numerics;

namespace InfiniteDecimal.Test;

public class AdditionalIntegralConversionTest
{
    public enum ConversionRoute { Interface, ConvertObject, ToType, ChangeType }

    public static IEnumerable<object[]> BoundaryCases()
    {
        var bounds = new (Type type, BigInteger minimum, BigInteger maximum)[]
        {
            (typeof(sbyte), sbyte.MinValue, sbyte.MaxValue),
            (typeof(byte), byte.MinValue, byte.MaxValue),
            (typeof(short), short.MinValue, short.MaxValue),
            (typeof(ushort), ushort.MinValue, ushort.MaxValue),
            (typeof(int), int.MinValue, int.MaxValue),
            (typeof(uint), uint.MinValue, uint.MaxValue),
            (typeof(long), long.MinValue, long.MaxValue),
            (typeof(ulong), ulong.MinValue, ulong.MaxValue),
        };

        foreach (var (type, minimum, maximum) in bounds)
        {
            var min = minimum.ToString(CultureInfo.InvariantCulture);
            var max = maximum.ToString(CultureInfo.InvariantCulture);
            var negativeMin = minimum.IsZero ? "-0" : min;
            // Just below/above a midpoint, beyond decimal and double precision.
            // Every minimum is even and every maximum is odd.
            var cases = new (string input, string? expected)[]
            {
                (negativeMin + ".499999999999999999999999999999", min),
                (negativeMin + ".500000000000000000000000000001", null),
                (max + ".499999999999999999999999999999", max),
                (max + ".500000000000000000000000000001", null),
                ("1" + new string('0', 100), null),
                ("-1" + new string('0', 100), null),
                ("1.499999999999999999999999999999", "1"),
                ("2.500000000000000000000000000001", "3"),
                ("-0.499999999999999999999999999999", "0"),
            };

            foreach (var (input, expected) in cases)
            foreach (var route in Enum.GetValues<ConversionRoute>())
                yield return new object[] { type, input, expected!, route };
        }
    }

    [Theory]
    [MemberData(nameof(BoundaryCases))]
    public void IntegralConversions_PreserveBoundaryDigitsAndCheckTheRoundedValue(
        Type destination, string text, string? expectedText, ConversionRoute route)
    {
        var value = BigDec.Parse(text);
        if (expectedText is null)
        {
            Assert.Throws<OverflowException>(() => ConvertValue(value, destination, route));
            return;
        }

        // The oracle uses only BCL string-to-integer conversion, never BigDec rounding.
        var expected = Convert.ChangeType(expectedText, destination, CultureInfo.InvariantCulture);
        var actual = ConvertValue(value, destination, route);
        Assert.Equal(destination, actual.GetType());
        Assert.Equal(expected, actual);
    }

    private static object ConvertValue(BigDec value, Type destination, ConversionRoute route)
    {
        // Null providers exercise the default-provider path too.
        if (route == ConversionRoute.ToType)
            return ((IConvertible)value).ToType(destination, null);
        if (route == ConversionRoute.ChangeType)
            return Convert.ChangeType(value, destination, null);

        if (route == ConversionRoute.ConvertObject)
        {
            object boxed = value;
            return Type.GetTypeCode(destination) switch
            {
                TypeCode.SByte => (object)Convert.ToSByte(boxed),
                TypeCode.Byte => Convert.ToByte(boxed),
                TypeCode.Int16 => Convert.ToInt16(boxed),
                TypeCode.UInt16 => Convert.ToUInt16(boxed),
                TypeCode.Int32 => Convert.ToInt32(boxed),
                TypeCode.UInt32 => Convert.ToUInt32(boxed),
                TypeCode.Int64 => Convert.ToInt64(boxed),
                TypeCode.UInt64 => Convert.ToUInt64(boxed),
                _ => throw new ArgumentOutOfRangeException(nameof(destination)),
            };
        }

        IConvertible convertible = value;
        return Type.GetTypeCode(destination) switch
        {
            TypeCode.SByte => (object)convertible.ToSByte(null),
            TypeCode.Byte => convertible.ToByte(null),
            TypeCode.Int16 => convertible.ToInt16(null),
            TypeCode.UInt16 => convertible.ToUInt16(null),
            TypeCode.Int32 => convertible.ToInt32(null),
            TypeCode.UInt32 => convertible.ToUInt32(null),
            TypeCode.Int64 => convertible.ToInt64(null),
            TypeCode.UInt64 => convertible.ToUInt64(null),
            _ => throw new ArgumentOutOfRangeException(nameof(destination)),
        };
    }

    [Theory]
    [InlineData(typeof(int), "2147483647.999", "2147483647")]
    [InlineData(typeof(int), "-2147483648.999", "-2147483648")]
    [InlineData(typeof(long), "9223372036854775807.999", "9223372036854775807")]
    [InlineData(typeof(long), "-9223372036854775808.999", "-9223372036854775808")]
    [InlineData(typeof(ulong), "18446744073709551615.999", "18446744073709551615")]
    [InlineData(typeof(ulong), "-0.999", "0")]
    public void ExplicitCast_ChecksRangeAfterTruncation(Type destination, string text, string expectedText)
    {
        var expected = Convert.ChangeType(expectedText, destination, CultureInfo.InvariantCulture);
        Assert.Equal(expected, Cast(BigDec.Parse(text), destination));
    }

    [Theory]
    [InlineData(typeof(int), "2147483648.001")]
    [InlineData(typeof(int), "-2147483649.001")]
    [InlineData(typeof(long), "9223372036854775808.001")]
    [InlineData(typeof(long), "-9223372036854775809.001")]
    [InlineData(typeof(ulong), "18446744073709551616.001")]
    [InlineData(typeof(ulong), "-1.001")]
    public void ExplicitCast_ThrowsWhenTruncatedValueStillOverflows(Type destination, string text)
    {
        var value = BigDec.Parse(text);
        Assert.Throws<OverflowException>(() => Cast(value, destination));
    }

    private static object Cast(BigDec value, Type destination)
    {
        if (destination == typeof(int)) return (int)value;
        if (destination == typeof(long)) return (long)value;
        if (destination == typeof(ulong)) return (ulong)value;
        throw new ArgumentOutOfRangeException(nameof(destination));
    }
}
