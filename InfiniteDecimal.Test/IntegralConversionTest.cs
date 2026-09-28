using System.Globalization;
using System.Numerics;

namespace InfiniteDecimal.Test;

public class IntegralConversionTest
{
    public static IEnumerable<object[]> DestinationBounds()
    {
        return new object[][]
        {
            [typeof(sbyte), new BigInteger(sbyte.MinValue), new BigInteger(sbyte.MaxValue)],
            [typeof(byte), new BigInteger(byte.MinValue), new BigInteger(byte.MaxValue)],
            [typeof(short), new BigInteger(short.MinValue), new BigInteger(short.MaxValue)],
            [typeof(ushort), new BigInteger(ushort.MinValue), new BigInteger(ushort.MaxValue)],
            [typeof(int), new BigInteger(int.MinValue), new BigInteger(int.MaxValue)],
            [typeof(uint), new BigInteger(uint.MinValue), new BigInteger(uint.MaxValue)],
            [typeof(long), new BigInteger(long.MinValue), new BigInteger(long.MaxValue)],
            [typeof(ulong), new BigInteger(ulong.MinValue), new BigInteger(ulong.MaxValue)],
        };
    }

    public static IEnumerable<object[]> OutsideDestinationRange()
    {
        return DestinationBounds()
            .SelectMany(t =>
            {
                var type = t[0];
                var minValue = (BigInteger)t[1] - BigInteger.One;
                var maxValue = (BigInteger)t[2] + BigInteger.One;

                return new[]
                {
                    new object[] { type, minValue.ToString() },
                    new object[] { type, maxValue.ToString() },
                };
            });
    }

    public static IEnumerable<object[]> RoundedValuesInsideDestinationRange()
    {
        yield return new object[] { typeof(sbyte), "-128.5", "-128" };
        yield return new object[] { typeof(sbyte), "127.4", "127" };
        yield return new object[] { typeof(byte), "-0.5", "0" };
        yield return new object[] { typeof(byte), "255.4", "255" };
        yield return new object[] { typeof(short), "-32768.5", "-32768" };
        yield return new object[] { typeof(short), "32767.4", "32767" };
        yield return new object[] { typeof(ushort), "-0.5", "0" };
        yield return new object[] { typeof(ushort), "65535.4", "65535" };
        yield return new object[] { typeof(int), "-2147483648.5", "-2147483648" };
        yield return new object[] { typeof(int), "2147483647.4", "2147483647" };
        yield return new object[] { typeof(uint), "-0.5", "0" };
        yield return new object[] { typeof(uint), "4294967295.4", "4294967295" };
        yield return new object[] { typeof(long), "-9223372036854775808.5", "-9223372036854775808" };
        yield return new object[] { typeof(long), "9223372036854775807.4", "9223372036854775807" };
        yield return new object[] { typeof(ulong), "-0.5", "0" };
        yield return new object[] { typeof(ulong), "18446744073709551615.4", "18446744073709551615" };
    }

    public static IEnumerable<object[]> RoundedValuesOutsideDestinationRange()
    {
        yield return new object[] { typeof(sbyte), "-128.6" };
        yield return new object[] { typeof(sbyte), "127.5" };
        yield return new object[] { typeof(byte), "-0.6" };
        yield return new object[] { typeof(byte), "255.5" };
        yield return new object[] { typeof(short), "-32768.6" };
        yield return new object[] { typeof(short), "32767.5" };
        yield return new object[] { typeof(ushort), "-0.6" };
        yield return new object[] { typeof(ushort), "65535.5" };
        yield return new object[] { typeof(int), "-2147483648.6" };
        yield return new object[] { typeof(int), "2147483647.5" };
        yield return new object[] { typeof(uint), "-0.6" };
        yield return new object[] { typeof(uint), "4294967295.5" };
        yield return new object[] { typeof(long), "-9223372036854775808.6" };
        yield return new object[] { typeof(long), "9223372036854775807.5" };
        yield return new object[] { typeof(ulong), "-0.6" };
        yield return new object[] { typeof(ulong), "18446744073709551615.5" };
    }

    public static IEnumerable<object[]> ExplicitCastOverflowValues()
    {
        yield return new object[] { typeof(int), "-2147483649" };
        yield return new object[] { typeof(int), "2147483648" };
        yield return new object[] { typeof(long), "-9223372036854775809" };
        yield return new object[] { typeof(long), "9223372036854775808" };
        yield return new object[] { typeof(ulong), "-1" };
        yield return new object[] { typeof(ulong), "18446744073709551616" };
    }

    [Theory]
    [MemberData(nameof(DestinationBounds))]
    public void IntegralConversion_AcceptsExactDestinationBounds(
        Type destinationType,
        BigInteger minimum,
        BigInteger maximum
    )
    {
        AssertConversionSucceeds(destinationType, minimum.ToString(), minimum.ToString());
        AssertConversionSucceeds(destinationType, maximum.ToString(), maximum.ToString());
    }

    [Theory]
    [MemberData(nameof(OutsideDestinationRange))]
    public void IntegralConversion_ThrowsOverflowOutsideDestinationRange(
        Type destinationType,
        string value
    )
    {
        AssertConversionThrowsOverflow(destinationType, value);
    }

    [Theory]
    [MemberData(nameof(RoundedValuesInsideDestinationRange))]
    public void IntegralConversion_RoundsToEvenAtDestinationBoundary(
        Type destinationType,
        string value,
        string expectedValue
    )
    {
        AssertConversionSucceeds(destinationType, value, expectedValue);
    }

    [Theory]
    [MemberData(nameof(RoundedValuesOutsideDestinationRange))]
    public void IntegralConversion_ThrowsOverflowWhenRoundedValueIsOutsideDestinationRange(
        Type destinationType,
        string value
    )
    {
        AssertConversionThrowsOverflow(destinationType, value);
    }

    [Theory]
    [InlineData("1.6", 1, 2)]
    [InlineData("-1.6", -1, -2)]
    [InlineData("2.5", 2, 2)]
    [InlineData("-2.5", -2, -2)]
    public void ExplicitCastTruncatesWhileIConvertibleRoundsToEven(
        string value,
        int expectedTruncated,
        int expectedRounded
    )
    {
        var input = BigDec.Parse(value);

        Assert.Equal(new BigInteger(expectedTruncated), (BigInteger)input);
        Assert.Equal(expectedTruncated, (int)input);
        Assert.Equal((long)expectedTruncated, (long)input);
        Assert.Equal(expectedRounded, input.ToInt32(CultureInfo.InvariantCulture));
    }

    [Theory]
    [MemberData(nameof(ExplicitCastOverflowValues))]
    public void ExplicitIntegralCast_ThrowsOverflowOutsideDestinationRange(
        Type destinationType,
        string value
    )
    {
        var input = BigDec.Parse(value);

        var exception = Record.Exception(() => { _ = CastExplicitly(input, destinationType); });

        Assert.IsType<OverflowException>(exception);
    }

    private static void AssertConversionSucceeds(
        Type destinationType,
        string inputValue,
        string expectedValue
    )
    {
        var input = BigDec.Parse(inputValue);
        var expected = ParsePrimitive(destinationType, expectedValue);

        Assert.Equal(expected, ConvertDirectly(input, destinationType));
        Assert.Equal(expected, input.ToType(destinationType, CultureInfo.InvariantCulture));
        Assert.Equal(expected, Convert.ChangeType(input, destinationType, CultureInfo.InvariantCulture));
    }

    private static void AssertConversionThrowsOverflow(Type destinationType, string inputValue)
    {
        var input = BigDec.Parse(inputValue);
        var exceptions = new[]
        {
            Record.Exception(() => { _ = ConvertDirectly(input, destinationType); }),
            Record.Exception(() => { _ = input.ToType(destinationType, CultureInfo.InvariantCulture); }),
            Record.Exception(() => { _ = Convert.ChangeType(input, destinationType, CultureInfo.InvariantCulture); }),
        };

        Assert.All(exceptions, exception => Assert.IsType<OverflowException>(exception));
    }

    private static object ConvertDirectly(BigDec value, Type destinationType)
    {
        var provider = CultureInfo.InvariantCulture;

        if (destinationType == typeof(sbyte))
        {
            return value.ToSByte(provider);
        }

        if (destinationType == typeof(byte))
        {
            return value.ToByte(provider);
        }

        if (destinationType == typeof(short))
        {
            return value.ToInt16(provider);
        }

        if (destinationType == typeof(ushort))
        {
            return value.ToUInt16(provider);
        }

        if (destinationType == typeof(int))
        {
            return value.ToInt32(provider);
        }

        if (destinationType == typeof(uint))
        {
            return value.ToUInt32(provider);
        }

        if (destinationType == typeof(long))
        {
            return value.ToInt64(provider);
        }

        if (destinationType == typeof(ulong))
        {
            return value.ToUInt64(provider);
        }

        throw new ArgumentOutOfRangeException(nameof(destinationType));
    }

    private static object CastExplicitly(BigDec value, Type destinationType)
    {
        if (destinationType == typeof(int))
        {
            return (int)value;
        }

        if (destinationType == typeof(long))
        {
            return (long)value;
        }

        if (destinationType == typeof(ulong))
        {
            return (ulong)value;
        }

        throw new ArgumentOutOfRangeException(nameof(destinationType));
    }

    private static object ParsePrimitive(Type destinationType, string value)
    {
        var provider = CultureInfo.InvariantCulture;

        if (destinationType == typeof(sbyte))
        {
            return sbyte.Parse(value, provider);
        }

        if (destinationType == typeof(byte))
        {
            return byte.Parse(value, provider);
        }

        if (destinationType == typeof(short))
        {
            return short.Parse(value, provider);
        }

        if (destinationType == typeof(ushort))
        {
            return ushort.Parse(value, provider);
        }

        if (destinationType == typeof(int))
        {
            return int.Parse(value, provider);
        }

        if (destinationType == typeof(uint))
        {
            return uint.Parse(value, provider);
        }

        if (destinationType == typeof(long))
        {
            return long.Parse(value, provider);
        }

        if (destinationType == typeof(ulong))
        {
            return ulong.Parse(value, provider);
        }

        throw new ArgumentOutOfRangeException(nameof(destinationType));
    }
}
