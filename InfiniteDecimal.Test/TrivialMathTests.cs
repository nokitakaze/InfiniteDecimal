using System.Numerics;

namespace InfiniteDecimal.Test;

public class TrivialMathTests
{
    public static object[][] GetTrivialTestData()
    {
        var rawValues = new decimal[]
        {
            0m,
            0.0000000001m,
            0.2m,
            0.3m,
            0.8m,
            1m,
            1.000001m,
            1.999999m,
            2m,
            10m,
            1488m,
        };
        rawValues = rawValues
            .Concat(rawValues.Select(t => -t))
            .Distinct()
            .OrderBy(t => t)
            .ToArray();

        return rawValues
            .SelectMany(value1 => rawValues.Select(value2 => new object[] { value1, value2 }))
            .ToArray();
    }

    private static bool IsInt(decimal value)
    {
        if (value < 0)
        {
            value = -value;
        }

        var diff = value - Math.Floor(value);
        return diff == 0;
    }

    private static bool IsInt(decimal value1, decimal value2)
    {
        return IsInt(value1) && IsInt(value2);
    }

    #region math operations

    [Theory]
    [MemberData(nameof(GetTrivialTestData))]
    public void GetTrivialTest(decimal arg1, decimal arg2)
    {
        GetTrivialTest_Plus(arg1, arg2);
        GetTrivialTest_Minus(arg1, arg2);
    }

    private static void GetTrivialTest_Plus(decimal arg1, decimal arg2)
    {
        var expected = new BigDec(arg1 + arg2);
        var bArg1 = new BigDec(arg1);
        var bArg2 = new BigDec(arg2);
        {
            var actual = bArg1 + arg2;
            Assert.Equal(actual, expected);
        }

        {
            // 1
            var actual = bArg1 + (double)arg2;
            var diff = expected - actual;
            var diffDouble = Math.Abs((double)diff);
            Assert.InRange(diffDouble, 0d, 0.0000001d);

            // 2
            actual = (double)arg1 + bArg2;
            diff = expected - actual;
            diffDouble = Math.Abs((double)diff);
            Assert.InRange(diffDouble, 0d, 0.0000001d);
        }

        {
            // 1
            var actual = bArg1 + (float)arg2;
            var diff = expected - actual;
            var diffDouble = Math.Abs((float)(double)diff);
            Assert.InRange(diffDouble, 0d, 0.0001d);

            // 2
            actual = bArg1 + (float)arg2;
            diff = expected - actual;
            diffDouble = Math.Abs((float)(double)diff);
            Assert.InRange(diffDouble, 0d, 0.0001d);
        }

        if (IsInt(arg1, arg2))
        {
            GetTrivialTest_Plus_Int(arg1, arg2);
        }
    }

    private static void GetTrivialTest_Plus_Int(decimal arg1, decimal arg2)
    {
        var expected = new BigDec(arg1 + arg2);
        var bArg1 = new BigDec(arg1);
        var bArg2 = new BigDec(arg2);
        {
            var actual = bArg1 + new BigInteger(arg2);
            Assert.Equal(actual, expected);

            actual = arg1 + bArg2;
            Assert.Equal(actual, expected);
        }

        {
            var actual = bArg1 + (long)arg2;
            Assert.Equal(actual, expected);

            actual = (long)arg1 + bArg2;
            Assert.Equal(actual, expected);
        }

        {
            var actual = bArg1 + (int)arg2;
            Assert.Equal(actual, expected);

            actual = (int)arg1 + bArg2;
            Assert.Equal(actual, expected);
        }

        if ((arg1 >= 0) && (arg2 >= 0))
        {
            {
                var actual = bArg1 + (ulong)arg2;
                Assert.Equal(actual, expected);

                actual = (ulong)arg1 + bArg2;
                Assert.Equal(actual, expected);
            }

            {
                var actual = bArg1 + (uint)arg2;
                Assert.Equal(actual, expected);

                actual = (uint)arg1 + bArg2;
                Assert.Equal(actual, expected);
            }
        }
    }

    private static void GetTrivialTest_Minus(decimal arg1, decimal arg2)
    {
        var expected = new BigDec(arg1 - arg2);
        var bArg1 = new BigDec(arg1);
        var bArg2 = new BigDec(arg2);
        {
            var actual = bArg1 - arg2;
            Assert.Equal(actual, expected);

            actual = arg1 - bArg2;
            Assert.Equal(actual, expected);
        }

        {
            // 1
            var actual = bArg1 - (double)arg2;
            var diff = expected - actual;
            var diffDouble = Math.Abs((double)diff);
            Assert.InRange(diffDouble, 0d, 0.0000001d);

            // 2
            actual = (double)arg1 - bArg2;
            diff = expected - actual;
            diffDouble = Math.Abs((double)diff);
            Assert.InRange(diffDouble, 0d, 0.0000001d);
        }

        {
            // 1
            var actual = bArg1 - (float)arg2;
            var diff = expected - actual;
            var diffDouble = Math.Abs((float)(double)diff);
            Assert.InRange(diffDouble, 0d, 0.0001d);

            // 2
            actual = (float)arg1 - bArg2;
            diff = expected - actual;
            diffDouble = Math.Abs((float)(double)diff);
            Assert.InRange(diffDouble, 0d, 0.0001d);
        }

        if (IsInt(arg1, arg2))
        {
            GetTrivialTest_Minus_Int(arg1, arg2);
        }
    }

    private static void GetTrivialTest_Minus_Int(decimal arg1, decimal arg2)
    {
        var expected = new BigDec(arg1 - arg2);
        var bArg1 = new BigDec(arg1);
        var bArg2 = new BigDec(arg2);
        {
            var actual = bArg1 - new BigInteger(arg2);
            Assert.Equal(actual, expected);

            actual = new BigInteger(arg1) - bArg2;
            Assert.Equal(actual, expected);
        }

        {
            var actual = bArg1 - (long)arg2;
            Assert.Equal(actual, expected);
        }

        {
            var actual = bArg1 - (int)arg2;
            Assert.Equal(actual, expected);

            actual = (int)arg1 - bArg2;
            Assert.Equal(actual, expected);
        }

        if ((arg1 >= 0) && (arg2 >= 0) && (arg1 >= arg2))
        {
            {
                var actual = bArg1 - (ulong)arg2;
                Assert.Equal(actual, expected);

                actual = (ulong)arg1 - bArg2;
                Assert.Equal(actual, expected);
            }

            {
                var actual = bArg1 - (uint)arg2;
                Assert.Equal(actual, expected);

                actual = (uint)arg1 - bArg2;
                Assert.Equal(actual, expected);
            }
        }
    }

    #endregion
}