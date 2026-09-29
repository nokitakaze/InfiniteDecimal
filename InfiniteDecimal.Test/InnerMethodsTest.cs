using System.Numerics;

namespace InfiniteDecimal.Test;

public class InnerMethodsTest
{
    #region ReduceTrailingZeroesTest

    public static object[][] ReduceTrailingZeroesTestData()
    {
        var testCases = new List<object[]>();
        testCases.Add([new BigInteger(123), 3, new BigInteger(123), 3]);
        testCases.Add([new BigInteger(120), 3, new BigInteger(12), 2]);
        testCases.Add([new BigInteger(1230), 4, new BigInteger(123), 3]);
        testCases.Add([new BigInteger(1000), 4, new BigInteger(1), 1]);
        testCases.Add([new BigInteger(1000), 3, new BigInteger(1), 0]);
        testCases.Add([new BigInteger(1000), 10, new BigInteger(1), 7]);
        testCases.Add([new BigInteger(100000), 4, new BigInteger(10), 0]);
        testCases.Add([new BigInteger(123), 10, new BigInteger(123), 10]);
        testCases.Add([new BigInteger(0), 0, new BigInteger(0), 0]);
        testCases.Add([new BigInteger(0), 10, new BigInteger(0), 0]);
        testCases.Add([new BigInteger(1001), 10, new BigInteger(1001), 10]);
        testCases.Add([new BigInteger(10010), 11, new BigInteger(1001), 10]);
        testCases.Add([new BigInteger(1), 0, new BigInteger(1), 0]);
        testCases.Add([new BigInteger(123), 0, new BigInteger(123), 0]);

        return testCases
            .SelectMany(item =>
            {
                var a0 = (BigInteger)item[0];
                if (a0.IsZero)
                {
                    return new[] { item };
                }

                var a2 = (BigInteger)item[2];
                return
                [
                    item,
                    [-a0, item[1], -a2, item[3]]
                ];
            })
            // ReSharper disable once UseCollectionExpression
            .ToArray();
    }

    [Theory]
    [MemberData(nameof(ReduceTrailingZeroesTestData))]
    public void ReduceTrailingZeroesTest(
        BigInteger mantissa,
        int offset,
        BigInteger expectedMantissa,
        int expectedOffset
    )
    {
        var expectedOffsetPower = BigDec.Pow10BigInt(expectedOffset);

        //
        var actualMantissa = mantissa;
        var actualOffset = offset;
        var actualOffsetPower = BigDec.Pow10BigInt(offset);
        BigDec.ReduceTrailingZeroes(ref actualMantissa, ref actualOffset, ref actualOffsetPower);

        Assert.Equal(expectedMantissa, actualMantissa);
        Assert.Equal(expectedOffset, actualOffset);
        Assert.Equal(expectedOffsetPower, actualOffsetPower);

        //
        actualMantissa = mantissa;
        actualOffset = offset;
        BigDec.ReduceTrailingZeroesWOPower(ref actualMantissa, ref actualOffset, out actualOffsetPower);

        Assert.Equal(expectedMantissa, actualMantissa);
        Assert.Equal(expectedOffset, actualOffset);
        Assert.Equal(expectedOffsetPower, actualOffsetPower);
    }

    #endregion

    #region

    public static IEnumerable<object[]> ReduceOverflowPrecisionData()
    {
        var rawTestCases = new List<(BigInteger input, int offset, int precision)>()
        {
            (new BigInteger(123), 3, 10),
            (new BigInteger(123), 3, 3),
            (new BigInteger(123), 4, 3),
            (new BigInteger(125), 4, 3),
            (new BigInteger(127), 4, 3),
            (new BigInteger(133), 4, 3),
            (new BigInteger(135), 4, 3),
            (new BigInteger(137), 4, 3),
            (new BigInteger(137), 4, 10),
            (new BigInteger(137), 10, 4),
        };

        return rawTestCases
            .SelectMany(item =>
            {
                return item.input.IsZero
                    ? [item]
                    : new[] { item, (input: -item.input, item.offset, item.precision) };
            })
            .Select(item =>
            {
                var pow = Enumerable
                    .Range(0, item.offset)
                    .Aggregate(1m, (a, _) => a * 10m);
                var powPrecision = Enumerable
                    .Range(0, item.precision)
                    .Aggregate(1m, (a, _) => a * 10m);

                var sign = item.input.Sign;
                var input = BigInteger.Abs(item.input);
                var value = (decimal)input / pow;

                var floor = Math.Floor(value * powPrecision) / powPrecision;
                var floor1 = floor;
                var floorOffset = 0;
                while (floor1 != Math.Floor(floor1))
                {
                    floorOffset++;
                    floor1 *= 10m;
                }

                var round = Math.Round((decimal)item.input / pow, item.precision);
                var round1 = round;
                var roundOffset = 0;
                while (round1 != Math.Floor(round1))
                {
                    roundOffset++;
                    round1 *= 10m;
                }

                return new object[]
                {
                    item.input,
                    item.offset,
                    item.precision,
                    sign * (BigInteger)floor1,
                    floorOffset,
                    (BigInteger)round1,
                    roundOffset,
                };
            })
            // ReSharper disable once UseCollectionExpression
            .ToArray();
    }

    [Theory]
    [MemberData(nameof(ReduceOverflowPrecisionData))]
    public void ReduceOverflowPrecisionTest(
        BigInteger mantissa,
        int offset,
        int maxPrecision,
        BigInteger expectedFloorMantissa,
        int expectedFloorOffset,
        BigInteger expectedRoundMantissa,
        int expectedRoundOffset
    )
    {
        //
        var actualMantissa = mantissa;
        var actualOffset = offset;
        var actualOffsetPower = BigDec.Pow10BigInt(offset);
        BigDec.ReduceOverflowPrecision(ref actualMantissa, ref actualOffset, ref actualOffsetPower, maxPrecision,
            roundingMode: 0);
        var expectedOffsetPower = BigDec.Pow10BigInt(expectedRoundOffset);

        Assert.Equal(expectedRoundMantissa, actualMantissa);
        Assert.Equal(expectedRoundOffset, actualOffset);
        Assert.Equal(expectedOffsetPower, actualOffsetPower);

        //
        actualMantissa = mantissa;
        actualOffset = offset;
        actualOffsetPower = BigDec.Pow10BigInt(offset);
        BigDec.ReduceOverflowPrecision(ref actualMantissa, ref actualOffset, ref actualOffsetPower, maxPrecision,
            roundingMode: 1);
        expectedOffsetPower = BigDec.Pow10BigInt(expectedFloorOffset);

        Assert.Equal(expectedFloorMantissa, actualMantissa);
        Assert.Equal(expectedFloorOffset, actualOffset);
        Assert.Equal(expectedOffsetPower, actualOffsetPower);
    }

    #endregion
}
