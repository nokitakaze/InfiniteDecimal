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

    public static bool IsInt(decimal value)
    {
        if (value < 0)
        {
            value = -value;
        }

        var diff = value - Math.Floor(value);
        return diff == 0;
    }

    public static bool IsInt(decimal value1, decimal value2)
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

    #region Check shorts

    public static object[][] CheckShortsData()
    {
        var values = new decimal[]
        {
            -0.003m,
            -1m,
            0m,
            1m,
            14.88m,
            10_000m,
            1_234_567_890_123_456m,
        };

        var modifiers = new long[]
        {
            byte.MinValue,
            byte.MaxValue,
            sbyte.MinValue,
            sbyte.MaxValue,
            ushort.MinValue,
            ushort.MaxValue,
            short.MinValue,
            short.MaxValue,
            uint.MinValue,
            uint.MaxValue,
            int.MinValue,
            int.MaxValue,
        };

        modifiers = modifiers
            .Concat(modifiers.Select(t => -t))
            .Distinct()
            .OrderBy(t => t)
            .ToArray();

        return values
            .SelectMany(value => modifiers.Select(modifier => new object[] { value, modifier }))
            .ToArray();
    }

    [Theory]
    [MemberData(nameof(CheckShortsData))]
    public void CheckShorts(decimal value, long modifier)
    {
        #region byte

        if (modifier is >= byte.MinValue and <= byte.MaxValue)
        {
            var typed = (byte)modifier;
            var bValue = new BigDec(value);
            {
                var expected = value + modifier;
                var actual = bValue + typed;
                Assert.True(expected == actual);
                Assert.True(actual == expected);
                Assert.False(expected != actual);
                Assert.False(actual != expected);
                Assert.True(expected >= actual);
                Assert.True(actual >= expected);
                Assert.True(expected <= actual);
                Assert.True(actual <= expected);
                Assert.False(expected > actual);
                Assert.False(expected < actual);
                Assert.False(actual > expected);
                Assert.False(actual < expected);

                actual = typed + bValue;
                Assert.True(expected == actual);
                Assert.True(actual == expected);
                Assert.False(expected != actual);
                Assert.False(actual != expected);
                Assert.True(expected >= actual);
                Assert.True(actual >= expected);
                Assert.True(expected <= actual);
                Assert.True(actual <= expected);
                Assert.False(expected > actual);
                Assert.False(expected < actual);
                Assert.False(actual > expected);
                Assert.False(actual < expected);
            }

            {
                var expected = value - modifier;
                var actual = bValue - typed;
                Assert.True(expected == actual);
                Assert.True(actual == expected);
                Assert.False(expected != actual);
                Assert.False(actual != expected);
                Assert.True(expected >= actual);
                Assert.True(actual >= expected);
                Assert.True(expected <= actual);
                Assert.True(actual <= expected);
                Assert.False(expected > actual);
                Assert.False(expected < actual);
                Assert.False(actual > expected);
                Assert.False(actual < expected);

                expected = modifier - value;
                actual = typed - bValue;
                Assert.True(expected == actual);
                Assert.True(actual == expected);
                Assert.False(expected != actual);
                Assert.False(actual != expected);
                Assert.True(expected >= actual);
                Assert.True(actual >= expected);
                Assert.True(expected <= actual);
                Assert.True(actual <= expected);
                Assert.False(expected > actual);
                Assert.False(expected < actual);
                Assert.False(actual > expected);
                Assert.False(actual < expected);
            }

            {
                var expected = value * modifier;
                var actual = bValue * typed;
                Assert.True(expected == actual);
                Assert.True(actual == expected);
                Assert.False(expected != actual);
                Assert.False(actual != expected);
                Assert.True(expected >= actual);
                Assert.True(actual >= expected);
                Assert.True(expected <= actual);
                Assert.True(actual <= expected);
                Assert.False(expected > actual);
                Assert.False(expected < actual);
                Assert.False(actual > expected);
                Assert.False(actual < expected);

                actual = typed * bValue;
                Assert.True(expected == actual);
                Assert.True(actual == expected);
                Assert.False(expected != actual);
                Assert.False(actual != expected);
                Assert.True(expected >= actual);
                Assert.True(actual >= expected);
                Assert.True(expected <= actual);
                Assert.True(actual <= expected);
                Assert.False(expected > actual);
                Assert.False(expected < actual);
                Assert.False(actual > expected);
                Assert.False(actual < expected);
            }

            if (modifier != 0)
            {
                var expected = value / modifier;
                var actual = bValue / typed;
                var diff = (expected - actual).Abs();
                Assert.True(diff <= 0.000_000_1d);
            }

            if (value != 0)
            {
                var expected = modifier / value;
                var actual = typed / bValue;
                var diff = (expected - actual).Abs();
                Assert.True(diff <= 0.000_000_1d);
            }
        }

        #endregion

        #region sbyte

        if (modifier is >= sbyte.MinValue and <= sbyte.MaxValue)
        {
            var typed = (sbyte)modifier;
            var bValue = new BigDec(value);
            {
                var expected = value + modifier;
                var actual = bValue + typed;
                Assert.True(expected == actual);
                Assert.True(actual == expected);
                Assert.False(expected != actual);
                Assert.False(actual != expected);
                Assert.True(expected >= actual);
                Assert.True(actual >= expected);
                Assert.True(expected <= actual);
                Assert.True(actual <= expected);
                Assert.False(expected > actual);
                Assert.False(expected < actual);
                Assert.False(actual > expected);
                Assert.False(actual < expected);

                actual = typed + bValue;
                Assert.True(expected == actual);
                Assert.True(actual == expected);
                Assert.False(expected != actual);
                Assert.False(actual != expected);
                Assert.True(expected >= actual);
                Assert.True(actual >= expected);
                Assert.True(expected <= actual);
                Assert.True(actual <= expected);
                Assert.False(expected > actual);
                Assert.False(expected < actual);
                Assert.False(actual > expected);
                Assert.False(actual < expected);
            }

            {
                var expected = value - modifier;
                var actual = bValue - typed;
                Assert.True(expected == actual);
                Assert.True(actual == expected);
                Assert.False(expected != actual);
                Assert.False(actual != expected);
                Assert.True(expected >= actual);
                Assert.True(actual >= expected);
                Assert.True(expected <= actual);
                Assert.True(actual <= expected);
                Assert.False(expected > actual);
                Assert.False(expected < actual);
                Assert.False(actual > expected);
                Assert.False(actual < expected);

                expected = modifier - value;
                actual = typed - bValue;
                Assert.True(expected == actual);
                Assert.True(actual == expected);
                Assert.False(expected != actual);
                Assert.False(actual != expected);
                Assert.True(expected >= actual);
                Assert.True(actual >= expected);
                Assert.True(expected <= actual);
                Assert.True(actual <= expected);
                Assert.False(expected > actual);
                Assert.False(expected < actual);
                Assert.False(actual > expected);
                Assert.False(actual < expected);
            }

            {
                var expected = value * modifier;
                var actual = bValue * typed;
                Assert.True(expected == actual);
                Assert.True(actual == expected);
                Assert.False(expected != actual);
                Assert.False(actual != expected);
                Assert.True(expected >= actual);
                Assert.True(actual >= expected);
                Assert.True(expected <= actual);
                Assert.True(actual <= expected);
                Assert.False(expected > actual);
                Assert.False(expected < actual);
                Assert.False(actual > expected);
                Assert.False(actual < expected);

                actual = typed * bValue;
                Assert.True(expected == actual);
                Assert.True(actual == expected);
                Assert.False(expected != actual);
                Assert.False(actual != expected);
                Assert.True(expected >= actual);
                Assert.True(actual >= expected);
                Assert.True(expected <= actual);
                Assert.True(actual <= expected);
                Assert.False(expected > actual);
                Assert.False(expected < actual);
                Assert.False(actual > expected);
                Assert.False(actual < expected);
            }

            if (modifier != 0)
            {
                var expected = value / modifier;
                var actual = bValue / typed;
                var diff = (expected - actual).Abs();
                Assert.True(diff <= 0.000_000_1d);
            }

            if (value != 0)
            {
                var expected = modifier / value;
                var actual = typed / bValue;
                var diff = (expected - actual).Abs();
                Assert.True(diff <= 0.000_000_1d);
            }
        }

        #endregion

        #region ushort

        if (modifier is >= ushort.MinValue and <= ushort.MaxValue)
        {
            var typed = (ushort)modifier;
            var bValue = new BigDec(value);
            {
                var expected = value + modifier;
                var actual = bValue + typed;
                Assert.True(expected == actual);
                Assert.True(actual == expected);
                Assert.False(expected != actual);
                Assert.False(actual != expected);
                Assert.True(expected >= actual);
                Assert.True(actual >= expected);
                Assert.True(expected <= actual);
                Assert.True(actual <= expected);
                Assert.False(expected > actual);
                Assert.False(expected < actual);
                Assert.False(actual > expected);
                Assert.False(actual < expected);

                actual = typed + bValue;
                Assert.True(expected == actual);
                Assert.True(actual == expected);
                Assert.False(expected != actual);
                Assert.False(actual != expected);
                Assert.True(expected >= actual);
                Assert.True(actual >= expected);
                Assert.True(expected <= actual);
                Assert.True(actual <= expected);
                Assert.False(expected > actual);
                Assert.False(expected < actual);
                Assert.False(actual > expected);
                Assert.False(actual < expected);
            }

            {
                var expected = value - modifier;
                var actual = bValue - typed;
                Assert.True(expected == actual);
                Assert.True(actual == expected);
                Assert.False(expected != actual);
                Assert.False(actual != expected);
                Assert.True(expected >= actual);
                Assert.True(actual >= expected);
                Assert.True(expected <= actual);
                Assert.True(actual <= expected);
                Assert.False(expected > actual);
                Assert.False(expected < actual);
                Assert.False(actual > expected);
                Assert.False(actual < expected);

                expected = modifier - value;
                actual = typed - bValue;
                Assert.True(expected == actual);
                Assert.True(actual == expected);
                Assert.False(expected != actual);
                Assert.False(actual != expected);
                Assert.True(expected >= actual);
                Assert.True(actual >= expected);
                Assert.True(expected <= actual);
                Assert.True(actual <= expected);
                Assert.False(expected > actual);
                Assert.False(expected < actual);
                Assert.False(actual > expected);
                Assert.False(actual < expected);
            }

            {
                var expected = value * modifier;
                var actual = bValue * typed;
                Assert.True(expected == actual);
                Assert.True(actual == expected);
                Assert.False(expected != actual);
                Assert.False(actual != expected);
                Assert.True(expected >= actual);
                Assert.True(actual >= expected);
                Assert.True(expected <= actual);
                Assert.True(actual <= expected);
                Assert.False(expected > actual);
                Assert.False(expected < actual);
                Assert.False(actual > expected);
                Assert.False(actual < expected);

                actual = typed * bValue;
                Assert.True(expected == actual);
                Assert.True(actual == expected);
                Assert.False(expected != actual);
                Assert.False(actual != expected);
                Assert.True(expected >= actual);
                Assert.True(actual >= expected);
                Assert.True(expected <= actual);
                Assert.True(actual <= expected);
                Assert.False(expected > actual);
                Assert.False(expected < actual);
                Assert.False(actual > expected);
                Assert.False(actual < expected);
            }

            if (modifier != 0)
            {
                var expected = value / modifier;
                var actual = bValue / typed;
                var diff = (expected - actual).Abs();
                Assert.True(diff <= 0.000_000_1d);
            }

            if (value != 0)
            {
                var expected = modifier / value;
                var actual = typed / bValue;
                var diff = (expected - actual).Abs();
                Assert.True(diff <= 0.000_000_1d);
            }
        }

        #endregion

        #region short

        if (modifier is >= short.MinValue and <= short.MaxValue)
        {
            var typed = (short)modifier;
            var bValue = new BigDec(value);
            {
                var expected = value + modifier;
                var actual = bValue + typed;
                Assert.True(expected == actual);
                Assert.True(actual == expected);
                Assert.False(expected != actual);
                Assert.False(actual != expected);
                Assert.True(expected >= actual);
                Assert.True(actual >= expected);
                Assert.True(expected <= actual);
                Assert.True(actual <= expected);
                Assert.False(expected > actual);
                Assert.False(expected < actual);
                Assert.False(actual > expected);
                Assert.False(actual < expected);

                actual = typed + bValue;
                Assert.True(expected == actual);
                Assert.True(actual == expected);
                Assert.False(expected != actual);
                Assert.False(actual != expected);
                Assert.True(expected >= actual);
                Assert.True(actual >= expected);
                Assert.True(expected <= actual);
                Assert.True(actual <= expected);
                Assert.False(expected > actual);
                Assert.False(expected < actual);
                Assert.False(actual > expected);
                Assert.False(actual < expected);
            }

            {
                var expected = value - modifier;
                var actual = bValue - typed;
                Assert.True(expected == actual);
                Assert.True(actual == expected);
                Assert.False(expected != actual);
                Assert.False(actual != expected);
                Assert.True(expected >= actual);
                Assert.True(actual >= expected);
                Assert.True(expected <= actual);
                Assert.True(actual <= expected);
                Assert.False(expected > actual);
                Assert.False(expected < actual);
                Assert.False(actual > expected);
                Assert.False(actual < expected);

                expected = modifier - value;
                actual = typed - bValue;
                Assert.True(expected == actual);
                Assert.True(actual == expected);
                Assert.False(expected != actual);
                Assert.False(actual != expected);
                Assert.True(expected >= actual);
                Assert.True(actual >= expected);
                Assert.True(expected <= actual);
                Assert.True(actual <= expected);
                Assert.False(expected > actual);
                Assert.False(expected < actual);
                Assert.False(actual > expected);
                Assert.False(actual < expected);
            }

            {
                var expected = value * modifier;
                var actual = bValue * typed;
                Assert.True(expected == actual);
                Assert.True(actual == expected);
                Assert.False(expected != actual);
                Assert.False(actual != expected);
                Assert.True(expected >= actual);
                Assert.True(actual >= expected);
                Assert.True(expected <= actual);
                Assert.True(actual <= expected);
                Assert.False(expected > actual);
                Assert.False(expected < actual);
                Assert.False(actual > expected);
                Assert.False(actual < expected);

                actual = typed * bValue;
                Assert.True(expected == actual);
                Assert.True(actual == expected);
                Assert.False(expected != actual);
                Assert.False(actual != expected);
                Assert.True(expected >= actual);
                Assert.True(actual >= expected);
                Assert.True(expected <= actual);
                Assert.True(actual <= expected);
                Assert.False(expected > actual);
                Assert.False(expected < actual);
                Assert.False(actual > expected);
                Assert.False(actual < expected);
            }

            if (modifier != 0)
            {
                var expected = value / modifier;
                var actual = bValue / typed;
                var diff = (expected - actual).Abs();
                Assert.True(diff <= 0.000_000_1d);
            }

            if (value != 0)
            {
                var expected = modifier / value;
                var actual = typed / bValue;
                var diff = (expected - actual).Abs();
                Assert.True(diff <= 0.000_000_1d);
            }
        }

        #endregion

        #region uint

        if (modifier is >= uint.MinValue and <= uint.MaxValue)
        {
            var typed = (uint)modifier;
            var bValue = new BigDec(value);
            {
                var expected = value + modifier;
                var actual = bValue + typed;
                Assert.True(expected == actual);
                Assert.True(actual == expected);
                Assert.False(expected != actual);
                Assert.False(actual != expected);
                Assert.True(expected >= actual);
                Assert.True(actual >= expected);
                Assert.True(expected <= actual);
                Assert.True(actual <= expected);
                Assert.False(expected > actual);
                Assert.False(expected < actual);
                Assert.False(actual > expected);
                Assert.False(actual < expected);

                actual = typed + bValue;
                Assert.True(expected == actual);
                Assert.True(actual == expected);
                Assert.False(expected != actual);
                Assert.False(actual != expected);
                Assert.True(expected >= actual);
                Assert.True(actual >= expected);
                Assert.True(expected <= actual);
                Assert.True(actual <= expected);
                Assert.False(expected > actual);
                Assert.False(expected < actual);
                Assert.False(actual > expected);
                Assert.False(actual < expected);
            }

            {
                var expected = value - modifier;
                var actual = bValue - typed;
                Assert.True(expected == actual);
                Assert.True(actual == expected);
                Assert.False(expected != actual);
                Assert.False(actual != expected);
                Assert.True(expected >= actual);
                Assert.True(actual >= expected);
                Assert.True(expected <= actual);
                Assert.True(actual <= expected);
                Assert.False(expected > actual);
                Assert.False(expected < actual);
                Assert.False(actual > expected);
                Assert.False(actual < expected);

                expected = modifier - value;
                actual = typed - bValue;
                Assert.True(expected == actual);
                Assert.True(actual == expected);
                Assert.False(expected != actual);
                Assert.False(actual != expected);
                Assert.True(expected >= actual);
                Assert.True(actual >= expected);
                Assert.True(expected <= actual);
                Assert.True(actual <= expected);
                Assert.False(expected > actual);
                Assert.False(expected < actual);
                Assert.False(actual > expected);
                Assert.False(actual < expected);
            }

            {
                var expected = value * modifier;
                var actual = bValue * typed;
                Assert.True(expected == actual);
                Assert.True(actual == expected);
                Assert.False(expected != actual);
                Assert.False(actual != expected);
                Assert.True(expected >= actual);
                Assert.True(actual >= expected);
                Assert.True(expected <= actual);
                Assert.True(actual <= expected);
                Assert.False(expected > actual);
                Assert.False(expected < actual);
                Assert.False(actual > expected);
                Assert.False(actual < expected);

                actual = typed * bValue;
                Assert.True(expected == actual);
                Assert.True(actual == expected);
                Assert.False(expected != actual);
                Assert.False(actual != expected);
                Assert.True(expected >= actual);
                Assert.True(actual >= expected);
                Assert.True(expected <= actual);
                Assert.True(actual <= expected);
                Assert.False(expected > actual);
                Assert.False(expected < actual);
                Assert.False(actual > expected);
                Assert.False(actual < expected);
            }

            if (modifier != 0)
            {
                var expected = value / modifier;
                var actual = bValue / typed;
                var diff = (expected - actual).Abs();
                Assert.True(diff <= 0.000_000_1d);
            }

            if (value != 0)
            {
                var expected = modifier / value;
                var actual = typed / bValue;
                var diff = (expected - actual).Abs();
                Assert.True(diff <= 0.000_000_1d);
            }
        }

        #endregion

        #region int

        if (modifier is >= int.MinValue and <= int.MaxValue)
        {
            var typed = (int)modifier;
            var bValue = new BigDec(value);
            {
                var expected = value + modifier;
                var actual = bValue + typed;
                Assert.True(expected == actual);
                Assert.True(actual == expected);
                Assert.False(expected != actual);
                Assert.False(actual != expected);
                Assert.True(expected >= actual);
                Assert.True(actual >= expected);
                Assert.True(expected <= actual);
                Assert.True(actual <= expected);
                Assert.False(expected > actual);
                Assert.False(expected < actual);
                Assert.False(actual > expected);
                Assert.False(actual < expected);

                actual = typed + bValue;
                Assert.True(expected == actual);
                Assert.True(actual == expected);
                Assert.False(expected != actual);
                Assert.False(actual != expected);
                Assert.True(expected >= actual);
                Assert.True(actual >= expected);
                Assert.True(expected <= actual);
                Assert.True(actual <= expected);
                Assert.False(expected > actual);
                Assert.False(expected < actual);
                Assert.False(actual > expected);
                Assert.False(actual < expected);
            }

            {
                var expected = value - modifier;
                var actual = bValue - typed;
                Assert.True(expected == actual);
                Assert.True(actual == expected);
                Assert.False(expected != actual);
                Assert.False(actual != expected);
                Assert.True(expected >= actual);
                Assert.True(actual >= expected);
                Assert.True(expected <= actual);
                Assert.True(actual <= expected);
                Assert.False(expected > actual);
                Assert.False(expected < actual);
                Assert.False(actual > expected);
                Assert.False(actual < expected);

                expected = modifier - value;
                actual = typed - bValue;
                Assert.True(expected == actual);
                Assert.True(actual == expected);
                Assert.False(expected != actual);
                Assert.False(actual != expected);
                Assert.True(expected >= actual);
                Assert.True(actual >= expected);
                Assert.True(expected <= actual);
                Assert.True(actual <= expected);
                Assert.False(expected > actual);
                Assert.False(expected < actual);
                Assert.False(actual > expected);
                Assert.False(actual < expected);
            }

            {
                var expected = value * modifier;
                var actual = bValue * typed;
                Assert.True(expected == actual);
                Assert.True(actual == expected);
                Assert.False(expected != actual);
                Assert.False(actual != expected);
                Assert.True(expected >= actual);
                Assert.True(actual >= expected);
                Assert.True(expected <= actual);
                Assert.True(actual <= expected);
                Assert.False(expected > actual);
                Assert.False(expected < actual);
                Assert.False(actual > expected);
                Assert.False(actual < expected);

                actual = typed * bValue;
                Assert.True(expected == actual);
                Assert.True(actual == expected);
                Assert.False(expected != actual);
                Assert.False(actual != expected);
                Assert.True(expected >= actual);
                Assert.True(actual >= expected);
                Assert.True(expected <= actual);
                Assert.True(actual <= expected);
                Assert.False(expected > actual);
                Assert.False(expected < actual);
                Assert.False(actual > expected);
                Assert.False(actual < expected);
            }

            if (modifier != 0)
            {
                var expected = value / modifier;
                var actual = bValue / typed;
                var diff = (expected - actual).Abs();
                Assert.True(diff <= 0.000_000_1d);
            }

            if (value != 0)
            {
                var expected = modifier / value;
                var actual = typed / bValue;
                var diff = (expected - actual).Abs();
                Assert.True(diff <= 0.000_000_1d);
            }
        }

        #endregion

        #region ulong

        if (modifier >= 0)
        {
            var typed = (ulong)modifier;
            var bValue = new BigDec(value);
            {
                var expected = value + modifier;
                var actual = bValue + typed;
                Assert.True(expected == actual);
                Assert.True(actual == expected);
                Assert.False(expected != actual);
                Assert.False(actual != expected);
                Assert.True(expected >= actual);
                Assert.True(actual >= expected);
                Assert.True(expected <= actual);
                Assert.True(actual <= expected);
                Assert.False(expected > actual);
                Assert.False(expected < actual);
                Assert.False(actual > expected);
                Assert.False(actual < expected);

                actual = typed + bValue;
                Assert.True(expected == actual);
                Assert.True(actual == expected);
                Assert.False(expected != actual);
                Assert.False(actual != expected);
                Assert.True(expected >= actual);
                Assert.True(actual >= expected);
                Assert.True(expected <= actual);
                Assert.True(actual <= expected);
                Assert.False(expected > actual);
                Assert.False(expected < actual);
                Assert.False(actual > expected);
                Assert.False(actual < expected);
            }

            {
                var expected = value - modifier;
                var actual = bValue - typed;
                Assert.True(expected == actual);
                Assert.True(actual == expected);
                Assert.False(expected != actual);
                Assert.False(actual != expected);
                Assert.True(expected >= actual);
                Assert.True(actual >= expected);
                Assert.True(expected <= actual);
                Assert.True(actual <= expected);
                Assert.False(expected > actual);
                Assert.False(expected < actual);
                Assert.False(actual > expected);
                Assert.False(actual < expected);

                expected = modifier - value;
                actual = typed - bValue;
                Assert.True(expected == actual);
                Assert.True(actual == expected);
                Assert.False(expected != actual);
                Assert.False(actual != expected);
                Assert.True(expected >= actual);
                Assert.True(actual >= expected);
                Assert.True(expected <= actual);
                Assert.True(actual <= expected);
                Assert.False(expected > actual);
                Assert.False(expected < actual);
                Assert.False(actual > expected);
                Assert.False(actual < expected);
            }

            {
                var expected = value * modifier;
                var actual = bValue * typed;
                Assert.True(expected == actual);
                Assert.True(actual == expected);
                Assert.False(expected != actual);
                Assert.False(actual != expected);
                Assert.True(expected >= actual);
                Assert.True(actual >= expected);
                Assert.True(expected <= actual);
                Assert.True(actual <= expected);
                Assert.False(expected > actual);
                Assert.False(expected < actual);
                Assert.False(actual > expected);
                Assert.False(actual < expected);

                actual = typed * bValue;
                Assert.True(expected == actual);
                Assert.True(actual == expected);
                Assert.False(expected != actual);
                Assert.False(actual != expected);
                Assert.True(expected >= actual);
                Assert.True(actual >= expected);
                Assert.True(expected <= actual);
                Assert.True(actual <= expected);
                Assert.False(expected > actual);
                Assert.False(expected < actual);
                Assert.False(actual > expected);
                Assert.False(actual < expected);
            }

            if (modifier != 0)
            {
                var expected = value / modifier;
                var actual = bValue / typed;
                var diff = (expected - actual).Abs();
                Assert.True(diff <= 0.000_000_1d);
            }

            if (value != 0)
            {
                var expected = modifier / value;
                var actual = typed / bValue;
                var diff = (expected - actual).Abs();
                Assert.True(diff <= 0.000_000_1d);
            }
        }

        #endregion

        #region long

        {
            // ReSharper disable once RedundantCast
            var typed = (long)modifier;
            var bValue = new BigDec(value);
            {
                var expected = value + modifier;
                var actual = bValue + typed;
                Assert.True(expected == actual);
                Assert.True(actual == expected);
                Assert.False(expected != actual);
                Assert.False(actual != expected);
                Assert.True(expected >= actual);
                Assert.True(actual >= expected);
                Assert.True(expected <= actual);
                Assert.True(actual <= expected);
                Assert.False(expected > actual);
                Assert.False(expected < actual);
                Assert.False(actual > expected);
                Assert.False(actual < expected);

                actual = typed + bValue;
                Assert.True(expected == actual);
                Assert.True(actual == expected);
                Assert.False(expected != actual);
                Assert.False(actual != expected);
                Assert.True(expected >= actual);
                Assert.True(actual >= expected);
                Assert.True(expected <= actual);
                Assert.True(actual <= expected);
                Assert.False(expected > actual);
                Assert.False(expected < actual);
                Assert.False(actual > expected);
                Assert.False(actual < expected);
            }

            {
                var expected = value - modifier;
                var actual = bValue - typed;
                Assert.True(expected == actual);
                Assert.True(actual == expected);
                Assert.False(expected != actual);
                Assert.False(actual != expected);
                Assert.True(expected >= actual);
                Assert.True(actual >= expected);
                Assert.True(expected <= actual);
                Assert.True(actual <= expected);
                Assert.False(expected > actual);
                Assert.False(expected < actual);
                Assert.False(actual > expected);
                Assert.False(actual < expected);

                expected = modifier - value;
                actual = typed - bValue;
                Assert.True(expected == actual);
                Assert.True(actual == expected);
                Assert.False(expected != actual);
                Assert.False(actual != expected);
                Assert.True(expected >= actual);
                Assert.True(actual >= expected);
                Assert.True(expected <= actual);
                Assert.True(actual <= expected);
                Assert.False(expected > actual);
                Assert.False(expected < actual);
                Assert.False(actual > expected);
                Assert.False(actual < expected);
            }

            {
                var expected = value * modifier;
                var actual = bValue * typed;
                Assert.True(expected == actual);
                Assert.True(actual == expected);
                Assert.False(expected != actual);
                Assert.False(actual != expected);
                Assert.True(expected >= actual);
                Assert.True(actual >= expected);
                Assert.True(expected <= actual);
                Assert.True(actual <= expected);
                Assert.False(expected > actual);
                Assert.False(expected < actual);
                Assert.False(actual > expected);
                Assert.False(actual < expected);

                actual = typed * bValue;
                Assert.True(expected == actual);
                Assert.True(actual == expected);
                Assert.False(expected != actual);
                Assert.False(actual != expected);
                Assert.True(expected >= actual);
                Assert.True(actual >= expected);
                Assert.True(expected <= actual);
                Assert.True(actual <= expected);
                Assert.False(expected > actual);
                Assert.False(expected < actual);
                Assert.False(actual > expected);
                Assert.False(actual < expected);
            }

            if (modifier != 0)
            {
                var expected = value / modifier;
                var actual = bValue / typed;
                var diff = (expected - actual).Abs();
                Assert.True(diff <= 0.000_000_1d);
            }

            if (value != 0)
            {
                var expected = modifier / value;
                var actual = typed / bValue;
                var diff = (expected - actual).Abs();
                Assert.True(diff <= 0.000_000_1d);
            }
        }

        #endregion

        #region BigInteger

        if (IsInt(modifier))
        {
            var typed = new BigInteger(modifier);
            var bValue = new BigDec(value);
            {
                var expected = value + modifier;
                var actual = bValue + typed;
                Assert.True(expected == actual);
                Assert.True(actual == expected);
                Assert.False(expected != actual);
                Assert.False(actual != expected);
                Assert.True(expected >= actual);
                Assert.True(actual >= expected);
                Assert.True(expected <= actual);
                Assert.True(actual <= expected);
                Assert.False(expected > actual);
                Assert.False(expected < actual);
                Assert.False(actual > expected);
                Assert.False(actual < expected);

                actual = typed + bValue;
                Assert.True(expected == actual);
                Assert.True(actual == expected);
                Assert.False(expected != actual);
                Assert.False(actual != expected);
                Assert.True(expected >= actual);
                Assert.True(actual >= expected);
                Assert.True(expected <= actual);
                Assert.True(actual <= expected);
                Assert.False(expected > actual);
                Assert.False(expected < actual);
                Assert.False(actual > expected);
                Assert.False(actual < expected);
            }

            {
                var expected = value - modifier;
                var actual = bValue - typed;
                Assert.True(expected == actual);
                Assert.True(actual == expected);
                Assert.False(expected != actual);
                Assert.False(actual != expected);
                Assert.True(expected >= actual);
                Assert.True(actual >= expected);
                Assert.True(expected <= actual);
                Assert.True(actual <= expected);
                Assert.False(expected > actual);
                Assert.False(expected < actual);
                Assert.False(actual > expected);
                Assert.False(actual < expected);

                expected = modifier - value;
                actual = typed - bValue;
                Assert.True(expected == actual);
                Assert.True(actual == expected);
                Assert.False(expected != actual);
                Assert.False(actual != expected);
                Assert.True(expected >= actual);
                Assert.True(actual >= expected);
                Assert.True(expected <= actual);
                Assert.True(actual <= expected);
                Assert.False(expected > actual);
                Assert.False(expected < actual);
                Assert.False(actual > expected);
                Assert.False(actual < expected);
            }

            {
                var expected = value * modifier;
                var actual = bValue * typed;
                Assert.True(expected == actual);
                Assert.True(actual == expected);
                Assert.False(expected != actual);
                Assert.False(actual != expected);
                Assert.True(expected >= actual);
                Assert.True(actual >= expected);
                Assert.True(expected <= actual);
                Assert.True(actual <= expected);
                Assert.False(expected > actual);
                Assert.False(expected < actual);
                Assert.False(actual > expected);
                Assert.False(actual < expected);

                actual = typed * bValue;
                Assert.True(expected == actual);
                Assert.True(actual == expected);
                Assert.False(expected != actual);
                Assert.False(actual != expected);
                Assert.True(expected >= actual);
                Assert.True(actual >= expected);
                Assert.True(expected <= actual);
                Assert.True(actual <= expected);
                Assert.False(expected > actual);
                Assert.False(expected < actual);
                Assert.False(actual > expected);
                Assert.False(actual < expected);
            }

            if (modifier != 0)
            {
                var expected = value / modifier;
                var actual = bValue / typed;
                var diff = (expected - actual).Abs();
                Assert.True(diff <= 0.000_000_1d);
            }

            if (value != 0)
            {
                var expected = modifier / value;
                var actual = typed / bValue;
                var diff = (expected - actual).Abs();
                Assert.True(diff <= 0.000_000_1d);
            }
        }

        #endregion

        #region decimal

        {
            var typed = (decimal)modifier;
            var bValue = new BigDec(value);
            {
                var expected = value + modifier;
                var actual = bValue + typed;
                Assert.True(expected == actual);
                Assert.True(actual == expected);
                Assert.False(expected != actual);
                Assert.False(actual != expected);
                Assert.True(expected >= actual);
                Assert.True(actual >= expected);
                Assert.True(expected <= actual);
                Assert.True(actual <= expected);
                Assert.False(expected > actual);
                Assert.False(expected < actual);
                Assert.False(actual > expected);
                Assert.False(actual < expected);

                actual = typed + bValue;
                Assert.True(expected == actual);
                Assert.True(actual == expected);
                Assert.False(expected != actual);
                Assert.False(actual != expected);
                Assert.True(expected >= actual);
                Assert.True(actual >= expected);
                Assert.True(expected <= actual);
                Assert.True(actual <= expected);
                Assert.False(expected > actual);
                Assert.False(expected < actual);
                Assert.False(actual > expected);
                Assert.False(actual < expected);
            }

            {
                var expected = value - modifier;
                var actual = bValue - typed;
                Assert.True(expected == actual);
                Assert.True(actual == expected);
                Assert.False(expected != actual);
                Assert.False(actual != expected);
                Assert.True(expected >= actual);
                Assert.True(actual >= expected);
                Assert.True(expected <= actual);
                Assert.True(actual <= expected);
                Assert.False(expected > actual);
                Assert.False(expected < actual);
                Assert.False(actual > expected);
                Assert.False(actual < expected);

                expected = modifier - value;
                actual = typed - bValue;
                Assert.True(expected == actual);
                Assert.True(actual == expected);
                Assert.False(expected != actual);
                Assert.False(actual != expected);
                Assert.True(expected >= actual);
                Assert.True(actual >= expected);
                Assert.True(expected <= actual);
                Assert.True(actual <= expected);
                Assert.False(expected > actual);
                Assert.False(expected < actual);
                Assert.False(actual > expected);
                Assert.False(actual < expected);
            }

            {
                var expected = value * modifier;
                var actual = bValue * typed;
                Assert.True(expected == actual);
                Assert.True(actual == expected);
                Assert.False(expected != actual);
                Assert.False(actual != expected);
                Assert.True(expected >= actual);
                Assert.True(actual >= expected);
                Assert.True(expected <= actual);
                Assert.True(actual <= expected);
                Assert.False(expected > actual);
                Assert.False(expected < actual);
                Assert.False(actual > expected);
                Assert.False(actual < expected);

                actual = typed * bValue;
                Assert.True(expected == actual);
                Assert.True(actual == expected);
                Assert.False(expected != actual);
                Assert.False(actual != expected);
                Assert.True(expected >= actual);
                Assert.True(actual >= expected);
                Assert.True(expected <= actual);
                Assert.True(actual <= expected);
                Assert.False(expected > actual);
                Assert.False(expected < actual);
                Assert.False(actual > expected);
                Assert.False(actual < expected);
            }

            if (modifier != 0)
            {
                var expected = value / modifier;
                var actual = bValue / typed;
                var diff = (expected - actual).Abs();
                Assert.True(diff <= 0.000_000_1d);
            }

            if (value != 0)
            {
                var expected = modifier / value;
                var actual = typed / bValue;
                var diff = (expected - actual).Abs();
                Assert.True(diff <= 0.000_000_1d);
            }
        }

        #endregion

        #region double

        {
            var typed = (double)modifier;
            var bValue = new BigDec(value);
            {
                var expected = value + modifier;
                var actual = bValue + typed;
                var diff = (expected - actual).Abs();
                Assert.True(diff <= 0.000_000_1d);

                actual = typed + bValue;
                diff = (expected - actual).Abs();
                Assert.True(diff <= 0.000_000_1d);
            }

            {
                var expected = value - modifier;
                var actual = bValue - typed;
                var diff = (expected - actual).Abs();
                Assert.True(diff <= 0.000_000_1d);

                expected = modifier - value;
                actual = typed - bValue;
                diff = (expected - actual).Abs();
                Assert.True(diff <= 0.000_000_1d);
            }

            {
                var expected = value * modifier;
                var actual = bValue * typed;
                var diff = (expected - actual).Abs();
                Assert.True(diff <= 0.000_000_1d);

                actual = typed * bValue;
                diff = (expected - actual).Abs();
                Assert.True(diff <= 0.000_000_1d);
            }

            if (modifier != 0)
            {
                var expected = value / modifier;
                var actual = bValue / typed;
                var diff = (expected - actual).Abs();
                Assert.True(diff <= 0.000_000_1d);
            }

            if (value != 0)
            {
                var expected = modifier / value;
                var actual = typed / bValue;
                var diff = (expected - actual).Abs();
                Assert.True(diff <= 0.000_000_1d);
            }
        }

        #endregion

        #region float

        if (modifier is >= short.MinValue and <= short.MaxValue)
        {
            var typed = (float)modifier;
            var bValue = new BigDec(value);
            {
                var expected = value + modifier;
                var actual = bValue + typed;
                var diff = (expected - actual).Abs();
                Assert.True(diff <= 0.000_1d);

                actual = typed + bValue;
                diff = (expected - actual).Abs();
                Assert.True(diff <= 0.000_1d);
            }

            {
                var expected = value - modifier;
                var actual = bValue - typed;
                var diff = (expected - actual).Abs();
                Assert.True(diff <= 0.000_1d);

                expected = modifier - value;
                actual = typed - bValue;
                diff = (expected - actual).Abs();
                Assert.True(diff <= 0.000_1d);
            }

            {
                var expected = value * modifier;
                var actual = bValue * typed;
                var diff = (expected - actual).Abs();
                Assert.True(diff <= 0.000_1d);

                actual = typed * bValue;
                diff = (expected - actual).Abs();
                Assert.True(diff <= 0.000_1d);
            }

            if (modifier != 0)
            {
                var expected = value / modifier;
                var actual = bValue / typed;
                var diff = (expected - actual).Abs();
                Assert.True(diff <= 0.000_1d);
            }

            if (value != 0)
            {
                var expected = modifier / value;
                var actual = typed / bValue;
                var diff = (expected - actual).Abs();
                Assert.True(diff <= 0.000_1d);
            }
        }

        #endregion
    }

    #endregion
}