using System.Globalization;
using Xunit.Abstractions;

namespace InfiniteDecimal.Test;

public class DeconstructIEEE754
{
    private readonly ITestOutputHelper _testOutputHelper;

    public DeconstructIEEE754(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    #region

    private static object[][] TestDataStep1(int limit)
    {
        var values1 = new List<BigDec>();
        foreach (var s1 in new[] { -1, 1 })
        {
            for (var i1 = 1; i1 <= limit; i1++)
            {
                var b1 = new BigDec(i1) * 0.1m * s1;
                values1.Add(b1);
            }
        }

        return values1
            .Select(value1 => new object[] { value1 })
            .ToArray();
    }

    private static IList<BigDec> TestDataStep2(int limit)
    {
        var values2 = new List<BigDec>();
        for (var offset2 = 1; offset2 < limit; offset2++)
        {
            foreach (var s2 in new[] { -1, 1 })
            {
                for (var i2 = 1; i2 <= limit; i2++)
                {
                    var b2 = new BigDec(i2) * BigDec.PowFracOfTen(offset2) * s2;
                    values2.Add(b2);
                }
            }
        }

        return values2.ToArray();
    }

    #region Deconstruct double

    public static object[][] TestSumDouble1Data()
    {
        return TestDataStep1(10);
    }

    [Theory]
    [MemberData(nameof(TestSumDouble1Data))]
    public void TestSumDouble1(BigDec b1)
    {
        foreach (var b2 in TestDataStep2(10))
        {
            _testOutputHelper.WriteLine("{0} + {1}", b1, b2);
            TestDoubleNumber(b1, b2);
        }
    }

    private void TestDoubleNumber(BigDec b1, BigDec b2)
    {
        var d1 = (double)b1;
        var d2 = (double)b2;

        var realSum = b1 + b2;
        var sumDouble = d1 + d2;
        var rounded = new BigDec(sumDouble);
        Assert.Equal(realSum, rounded);
    }

    #endregion

    #region Deconstruct float

    public static object[][] TestSumFloat1Data()
    {
        return TestDataStep1(7);
    }

    [Theory]
    [MemberData(nameof(TestSumFloat1Data))]
    public void TestSumFloat1(BigDec b1)
    {
        foreach (var b2 in TestDataStep2(6))
        {
            _testOutputHelper.WriteLine("{0} + {1}", b1, b2);
            TestSingleNumber(b1, b2);
        }
    }

    private void TestSingleNumber(BigDec b1, BigDec b2)
    {
        var f1 = (float)(double)b1;
        var f2 = (float)(double)b2;

        var realSum = b1 + b2;
        var sumFloat = f1 + f2;
        var rounded = new BigDec(sumFloat);
        Assert.Equal(realSum, rounded);
    }

    #endregion

    #region Picked cases

    [Fact]
    public void PickedTest()
    {
        // TestDoubleNumber(new BigDec(-0.2m), new BigDec(0.3m));
        // TestDoubleNumber(new BigDec(-0.1m), new BigDec(0.06m));
        TestSingleNumber(new BigDec(-0.3m), new BigDec(-0.0004m));
    }

    #endregion

    #endregion

    #region Convert decimal, double, float

    public static object[][] TestConvertDecimalData()
    {
        var values = new decimal[]
        {
            0m,
            2m,
            1_234_567_890m,
            1000.0001m,
            0.1m,
            0.000_000_1m,
            0.000_000_000_000_1m,
            0.100_1m,
            0.100_000_1m,
            0.100_000_001m,
            0.100_000_000_001m,
            0.100_000_000_000_001m,
            0.100_000_000_000_000_001m,
            0.100_2m,
            0.100_000_2m,
            0.100_000_002m,
            0.100_000_000_002m,
            0.100_000_000_000_002m,
            0.100_000_000_000_000_002m,
            0.3m,
            0.03m,
            0.003m,
            0.000_3m,
            0.000_03m,
            0.000_003m,
            0.000_000_3m,
            0.000_000_03m,
            0.000_000_003m,
            0.000_000_000_3m,
            1_234_567_890.000_000_000_000_1m,
            3.7m,
        };

        return values
            .Concat(values.Where(x => x > 0).Select(t => -t))
            .Select(t => new object[] { t })
            .ToArray();
    }

    public static object[][] TestConvertDoubleData()
    {
        return TestConvertDecimalData()
            .Select(t =>
            {
                var value = (decimal)t[0];
                if (Math.Abs(value).ToString(CultureInfo.InvariantCulture).Length > 18)
                {
                    return null;
                }

                // ReSharper disable once ConvertIfStatementToReturnStatement
                if (value is 0.100_000_000_000_001m or -0.100_000_000_000_001m or
                    0.100_000_000_000_002m or -0.100_000_000_000_002m)
                {
                    return null;
                }

                return t;
            })
            .Where(x => x is not null)
            .Cast<object[]>()
            .ToArray();
    }

    [Theory]
    [MemberData(nameof(TestConvertDecimalData))]
    public void StringifyDecimal(decimal rawValue)
    {
        var valueBio1 = new BigDec(rawValue);
        var stringify = valueBio1.ToStringDouble();
        var valueBio2 = BigDec.Parse(stringify);
        Assert.Equal(valueBio1, valueBio2);
    }

    [Theory]
    [MemberData(nameof(TestConvertDecimalData))]
    public void TestConvertDecimal(decimal rawValue)
    {
        var valueBio = new BigDec(rawValue);
        var valueString = valueBio.ToString(CultureInfo.InvariantCulture);
        var decimalRevert1 = (decimal)valueBio;
        var decimalRevert2 = decimal.Parse(valueString);
        var decimalRevert3 = (decimal)valueBio.ToType(typeof(decimal), null);

        Assert.Equal(rawValue, decimalRevert1);
        Assert.Equal(rawValue, decimalRevert2);
        Assert.Equal(rawValue, decimalRevert3);

        var valueBio2 = BigDec.Parse(valueString);
        Assert.Equal(valueBio, valueBio2);

        //
        Assert.True(rawValue == valueBio);
        Assert.False(rawValue != valueBio);
        Assert.True(valueBio == rawValue);
        Assert.False(valueBio != rawValue);

        if (rawValue == (long)rawValue)
        {
            var rawValueLong = (long)rawValue;
            Assert.True(valueBio == rawValueLong);
            Assert.False(valueBio != rawValueLong);
            Assert.True(rawValueLong == valueBio);
            Assert.False(rawValueLong != valueBio);
        }
    }

    [Theory]
    [MemberData(nameof(TestConvertDoubleData))]
    public void TestConvertDouble(decimal preRawValue)
    {
        var rawValue = (double)preRawValue;

        //
        var valueBio = new BigDec(rawValue);
        var valueString = valueBio.ToString(CultureInfo.InvariantCulture);
        var decimalRevert1 = (decimal)valueBio;
        var decimalRevert2 = decimal.Parse(valueString);
        var decimalRevert1a = (double)valueBio.ToType(typeof(double), null);

        Assert.Equal(preRawValue, decimalRevert1);
        Assert.Equal(preRawValue, decimalRevert2);
        Assert.InRange(decimalRevert1a, rawValue - 0.000000001d, rawValue + 0.000000001d);

        var valueBio2 = BigDec.Parse(valueString);
        Assert.Equal(valueBio, valueBio2);
    }

    public static object[][] TestConvertFloatData()
    {
        return TestConvertDecimalData()
            .Select(t =>
            {
                var value = (decimal)t[0];
                if (Math.Abs(value).ToString(CultureInfo.InvariantCulture).Length > 18)
                {
                    return null;
                }

                // ReSharper disable once ConvertIfStatementToReturnStatement
                if (value is 0.100_000_000_000_001m or -0.100_000_000_000_001m or
                    0.100_000_000_000_002m or -0.100_000_000_000_002m or 1000.0001m or -1000.0001m)
                {
                    return null;
                }

                return t;
            })
            .Where(x => x is not null)
            .Cast<object[]>()
            .ToArray();
    }

    [Theory]
    [MemberData(nameof(TestConvertFloatData))]
    public void TestConvertFloat(decimal preRawValue)
    {
        var rawValue = (float)preRawValue;

        //
        var valueBio = new BigDec(rawValue);
        var valueString = valueBio.ToString(CultureInfo.InvariantCulture);
        var decimalRevert1a = (float)valueBio.ToType(typeof(float), null);

        Assert.InRange(decimalRevert1a, rawValue - 0.000001f, rawValue + 0.000001f);

        var valueBio2 = BigDec.Parse(valueString);
        Assert.Equal(valueBio, valueBio2);
    }

    [Fact]
    public void TestConvertDoubleCycle()
    {
        for (var value = 0; value < 1000; value++)
        {
            for (var offset1 = 0; offset1 < 3; offset1++)
            {
                var pow1 = Enumerable
                    .Repeat(10m, offset1)
                    .Aggregate(1m, (a, b) => a * b);

                for (var offset2 = 0; offset2 < 7; offset2++)
                {
                    var pow2 = Enumerable
                        .Repeat(0.1m, offset2)
                        .Aggregate(1m, (a, b) => a * b);
                    decimal valueDecimal = (value * pow1) * pow2;
                    //  _testOutputHelper.WriteLine("Cycle: {0}\t{1}\t{2}", value, offset1, offset2);
                    TestConvertDouble(valueDecimal);
                }
            }
        }
    }

    #endregion

    #region ParseDouble

    public static object[][] ParseDoubleData()
    {
        var decimals = new decimal[] { 0, 1, 3, 4, 7, 9 };
        decimals = decimals
            .Concat(decimals.Select(t => -t))
            .Distinct()
            .OrderBy(t => t)
            .ToArray();

        return decimals
            .SelectMany(value1 => decimals.Select(value2 => new object[] { value1, value2 }))
            .ToArray();
    }

    [Theory]
    [MemberData(nameof(ParseDoubleData))]
    public void ParseDouble(decimal value1, decimal value2)
    {
        var pow = 10m;
        for (var i = 0; i < 16; i++)
        {
            pow /= 10m;

            var v1Dec = value1 * pow;
            var v2Dec = value2 * pow;

            var v1Double = (double)v1Dec;
            var v2Double = (double)v2Dec;

            var expected = new BigDec(v1Dec + v2Dec);
            var actualDouble = v1Double + v2Double;
            var actual = new BigDec(actualDouble);
            Assert.Equal(expected, actual);

            expected = new BigDec(v1Dec - v2Dec);
            actualDouble = v1Double - v2Double;
            actual = new BigDec(actualDouble);
            Assert.Equal(expected, actual);
        }
    }

    #endregion
}