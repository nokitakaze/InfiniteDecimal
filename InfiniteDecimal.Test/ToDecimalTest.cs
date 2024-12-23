using System.Globalization;
using System.Numerics;

namespace InfiniteDecimal.Test;

public class ToDecimalTest
{
    public static IEnumerable<object[]> RaiseExceptionOverflowData()
    {
        return new BigDec[]
            {
                BigDec.MaxDecimalValue + 1,
                BigDec.MaxDecimalValue + 10,
                BigDec.MaxDecimalValue + 100,
                BigDec.MaxDecimalValue * 2,
                BigDec.MaxDecimalValue.WithPrecision(BigDec.MaxDecimalValue.Offset + 6) * 1.000001m,
                BigDec.MinAbsDecimalValue.WithPrecision(BigDec.MinAbsDecimalValue.Offset + 10) * 0.999_999_999_9m,
                BigDec.MinAbsDecimalValue.WithPrecision(BigDec.MinAbsDecimalValue.Offset + 1) * 0.1m,
            }
            .SelectMany(t => new object[] { t, -t })
            .Select(t => new object[] { t });
    }

    [Theory]
    [MemberData(nameof(RaiseExceptionOverflowData))]
    public void RaiseExceptionOverflow(BigDec value)
    {
        Assert.Throws<System.OverflowException>(() => { _ = (decimal)value; });
        Assert.Throws<System.OverflowException>(() => { _ = value.ToDecimal(CultureInfo.InvariantCulture); });
        Assert.Throws<System.OverflowException>(() =>
        {
            _ = value.ToType(typeof(decimal), CultureInfo.InvariantCulture);
        });
    }

    public static IEnumerable<object[]> RaiseNoExceptionData()
    {
        var minAbsDecimalValue = BigDec.MinAbsDecimalValue.WithPrecision(BigDec.MinAbsDecimalValue.Offset + 18);
        return new BigDec[]
            {
                BigDec.One,
                BigDec.MaxDecimalValue,
                BigDec.MaxDecimalValue - 1,
                BigDec.MaxDecimalValue.WithPrecision(BigDec.MaxDecimalValue.Offset + 6) * 0.999_999m,
                BigDec.MinAbsDecimalValue,
                minAbsDecimalValue * (2 - 0.999_999_999_9m),
                BigDec.MinAbsDecimalValue * 10,
                minAbsDecimalValue * 1.1m,
                minAbsDecimalValue * 1.999_999_999m,
                minAbsDecimalValue * 1.999_999_999_999_999_999m,
                new BigDec(123_456_789_012_345m) + new BigDec(0.123_456_789_012_345m),
                new BigDec(123_456_789_012_345m) + new BigDec(0.123_456_789_012_345_678_901m),
                new BigDec(123_456_789_012m) + new BigDec(0.123_456_789_012_345_678_901_345m),
                new BigDec(123_456_789m) + new BigDec(0.123_456_789_012_345_678_901_345_012m),
                minAbsDecimalValue * 1_000_000.000_000_1m,
                minAbsDecimalValue * 10_000_000.000_000_01m,
            }
            .SelectMany(t => new object[] { t, -t })
            .Select(t => new object[] { t });
    }

    [Theory]
    [MemberData(nameof(RaiseNoExceptionData))]
    public void RaiseNoException(BigDec value)
    {
        var actualValues = new decimal[]
        {
            (decimal)value,
            value.ToDecimal(CultureInfo.InvariantCulture),
            (decimal)value.ToType(typeof(decimal), CultureInfo.InvariantCulture),
        };

        var log_10_2 = Math.Log2(10);
        var bodySizeInBits = BigInteger.Log(BigInteger.Abs(value.Mantissa), 2);
        double wrkBits;
        if (value.Offset > BigDec.MaxDecimalScale)
        {
            wrkBits = bodySizeInBits - Math.Max(value.Offset - BigDec.MaxDecimalScale, 0) * log_10_2;
            wrkBits = wrkBits switch
            {
                < 1 => 1,
                > 96 - 5 => 96 - 5,
                _ => wrkBits
            };
        }
        else
        {
            wrkBits = 96 - 5;
        }

        var wrkBitsInt = (int)Math.Floor(wrkBits);

        var range = new BigDec(BigInteger.One << (96 - wrkBitsInt)).WithPrecision(45) / (BigInteger.One << 96);
        var rangeMin = 1 - range;
        var rangeMax = 1 + range;

        foreach (var actual in actualValues)
        {
            var restored = new BigDec(actual);
            var r = restored / value;
            Assert.InRange(r, rangeMin, rangeMax);
        }
    }

    [Fact]
    public void RaiseNoExceptionOnZero()
    {
        var a1 = (decimal)BigDec.Zero;
        Assert.Equal(0m, a1);
        var a2 = BigDec.Zero.ToDecimal(CultureInfo.InvariantCulture);
        Assert.Equal(0m, a2);
        var a3 = (decimal)BigDec.Zero.ToType(typeof(decimal), CultureInfo.InvariantCulture);
        Assert.Equal(0m, a3);
    }

    [Fact]
    public void GetRealByteCount()
    {
        for (var bitCount = 95; bitCount < 96; bitCount++)
        {
            foreach (var modifier in new[] { -1, 0, 1 })
            {
                var realBitCount = bitCount + (modifier == -1 ? 0 : 1);
                var realByteCount = (int)Math.Ceiling(realBitCount * (1d / 8));

                var value = (BigInteger.One << bitCount) + modifier;
                var calculatedByteCount = BigDec.GetRealByteCount(value);
                Assert.Equal(realByteCount, calculatedByteCount);
                calculatedByteCount = BigDec.GetRealByteCount(-value);
                Assert.Equal(realByteCount, calculatedByteCount);
            }
        }
    }

    [Fact]
    public void GetRealByteCount_Zero()
    {
        Assert.Equal(1, BigDec.GetRealByteCount(BigInteger.Zero));
    }
}
