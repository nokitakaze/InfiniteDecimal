using System.Numerics;

namespace InfiniteDecimal.Test;

public class RoundingDirectionTest
{
    #region Core

    public static object[][] GetTestCases()
    {
        var two = new BigInteger(2);
        var three = new BigInteger(3);

        var rawCases = new (object input, BigInteger floor, BigInteger round, BigInteger ceil)[]
        {
            (0, BigInteger.Zero, BigInteger.Zero, BigInteger.Zero),
            (0f, BigInteger.Zero, BigInteger.Zero, BigInteger.Zero),
            (0m, BigInteger.Zero, BigInteger.Zero, BigInteger.Zero),
            (0d, BigInteger.Zero, BigInteger.Zero, BigInteger.Zero),

            (1, BigInteger.One, BigInteger.One, BigInteger.One),
            (1m, BigInteger.One, BigInteger.One, BigInteger.One),
            (1d, BigInteger.One, BigInteger.One, BigInteger.One),

            (-1, BigInteger.MinusOne, BigInteger.MinusOne, BigInteger.MinusOne),
            (-1m, BigInteger.MinusOne, BigInteger.MinusOne, BigInteger.MinusOne),
            (-1d, BigInteger.MinusOne, BigInteger.MinusOne, BigInteger.MinusOne),

            (2, two, two, two),
            (2m, two, two, two),
            (2d, two, two, two),

            (-2, -two, -two, -two),
            (-2m, -two, -two, -two),
            (-2d, -two, -two, -two),

            //
            (0.3d, BigInteger.Zero, BigInteger.Zero, BigInteger.One),
            (0.3m, BigInteger.Zero, BigInteger.Zero, BigInteger.One),
            (1.3d, BigInteger.One, BigInteger.One, two),
            (1.3m, BigInteger.One, BigInteger.One, two),
            (2.3d, two, two, three),
            (2.3m, two, two, three),

            //
            (0.5d, BigInteger.Zero, BigInteger.Zero, BigInteger.One),
            (0.5m, BigInteger.Zero, BigInteger.Zero, BigInteger.One),
            (1.5d, BigInteger.One, two, two),
            (1.5m, BigInteger.One, two, two),
            (2.5d, two, two, three),
            (2.5m, two, two, three),

            //
            (-0.3d, BigInteger.MinusOne, BigInteger.Zero, BigInteger.Zero),
            (-0.3m, BigInteger.MinusOne, BigInteger.Zero, BigInteger.Zero),
            (-1.3d, -two, BigInteger.MinusOne, BigInteger.MinusOne),
            (-1.3m, -two, BigInteger.MinusOne, BigInteger.MinusOne),
            (-2.3d, -three, -two, -two),
            (-2.3m, -three, -two, -two),

            //
            (-0.5d, BigInteger.MinusOne, BigInteger.Zero, BigInteger.Zero),
            (-0.5m, BigInteger.MinusOne, BigInteger.Zero, BigInteger.Zero),
            (-1.5d, -two, -two, BigInteger.MinusOne),
            (-1.5m, -two, -two, BigInteger.MinusOne),
            (-2.5d, -three, -two, -two),
            (-2.5m, -three, -two, -two),
        };

        return rawCases
            .Select(t => new[] { t.input, t.floor, t.round, t.ceil })
            .ToArray();
    }

    public static (bool type, object[][] values)[] GetTestCasesByType()
    {
        return GetTestCases()
            .Select(t =>
            {
                return t[0] switch
                {
                    double value => (type: value >= 0d, t),
                    float value => (type: value >= 0f, t),
                    decimal value => (type: value >= 0m, t),
                    int value => (type: value >= 0, t),
                    long value => (type: value >= 0, t),
                    _ => throw new ArgumentOutOfRangeException()
                };
            })
            .GroupBy(t => t.type)
            .Select(g => (type: g.Key, g.Select(t1 => t1.t).ToArray()))
            .ToArray();
    }

    #endregion

    #region Floor

    #region Positive

    public static IEnumerable<object[]> GetTestCases_FloorPositive()
    {
        return GetTestCasesByType()
            .First(x => x.type)
            .values
            .Select(t => new[] { t[0], t[1] });
    }

    [Theory]
    [MemberData(nameof(GetTestCases_FloorPositive))]
    void FloorPositive(object input, BigInteger expected)
    {
        BigDec bdInput = input switch
        {
            double value => new BigDec(value),
            float value => new BigDec(value),
            decimal value => new BigDec(value),
            long value => new BigDec(value),
            int value => new BigDec(value),
            _ => throw new ArgumentOutOfRangeException()
        };

        var actual = bdInput.Floor();
        Assert.Equal<BigInteger>(expected, actual);
    }

    #endregion

    #region Negative

    public static IEnumerable<object[]> GetTestCases_FloorNegative()
    {
        return GetTestCasesByType()
            .First(x => !x.type)
            .values
            .Select(t => new[] { t[0], t[1] });
    }

    [Theory]
    [MemberData(nameof(GetTestCases_FloorNegative))]
    void FloorNegative(object input, BigInteger expected)
    {
        BigDec bdInput = input switch
        {
            double value => new BigDec(value),
            float value => new BigDec(value),
            decimal value => new BigDec(value),
            long value => new BigDec(value),
            int value => new BigDec(value),
            _ => throw new ArgumentOutOfRangeException()
        };

        var actual = bdInput.Floor();
        Assert.Equal<BigInteger>(expected, actual);
    }

    #endregion

    #endregion

    #region Round

    #region Positive

    public static IEnumerable<object[]> GetTestCases_RoundPositive()
    {
        return GetTestCasesByType()
            .First(x => x.type)
            .values
            .Select(t => new[] { t[0], t[2] });
    }

    [Theory]
    [MemberData(nameof(GetTestCases_RoundPositive))]
    void RoundPositive(object input, BigInteger expected)
    {
        BigDec bdInput = input switch
        {
            double value => new BigDec(value),
            float value => new BigDec(value),
            decimal value => new BigDec(value),
            long value => new BigDec(value),
            int value => new BigDec(value),
            _ => throw new ArgumentOutOfRangeException()
        };

        var actual = (BigInteger)bdInput.Round(0);
        Assert.Equal<BigInteger>(expected, actual);
    }

    #endregion

    #region Negative

    public static IEnumerable<object[]> GetTestCases_RoundNegative()
    {
        return GetTestCasesByType()
            .First(x => !x.type)
            .values
            .Select(t => new[] { t[0], t[2] });
    }

    [Theory]
    [MemberData(nameof(GetTestCases_RoundNegative))]
    void RoundNegative(object input, BigInteger expected)
    {
        BigDec bdInput = input switch
        {
            double value => new BigDec(value),
            float value => new BigDec(value),
            decimal value => new BigDec(value),
            long value => new BigDec(value),
            int value => new BigDec(value),
            _ => throw new ArgumentOutOfRangeException()
        };

        var actual = (BigInteger)bdInput.Round(0);
        Assert.Equal<BigInteger>(expected, actual);
    }

    #endregion

    #endregion

    public static object[][] RoundNegative1_Data()
    {
        var testCases = new (decimal input, int numberCount, decimal expected)[]
        {
            (-0.33m, 1, -0.3m),
            (-0.05m, 1, 0m),
            (-0.07m, 1, -0.1m),
            //
            (-1.03m, 1, -1.0m),
            (-1.05m, 1, -1.0m),
            (-1.07m, 1, -1.1m),
            //
            (-0.43m, 1, -0.4m),
            (-0.47m, 1, -0.5m),
            //
            (-0.15m, 1, -0.2m),
            (-0.17m, 1, -0.2m),
            //
            (-1.13m, 1, -1.1m),
            (-1.15m, 1, -1.2m),
            (-1.17m, 1, -1.2m),
        };

        return testCases
            .Select(t => new object[] { t.input, t.numberCount, t.expected })
            .ToArray();
    }

    [Theory]
    [MemberData(nameof(RoundNegative1_Data))]
    void RoundNegative1(decimal input, int numberCount, decimal expected)
    {
        var actual = new BigDec(input).Round(numberCount);
        Assert.Equal(expected, (decimal)actual);
    }
}
