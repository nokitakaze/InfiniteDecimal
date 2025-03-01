﻿using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Numerics;
using System.Reflection;

namespace InfiniteDecimal.Test;

#pragma warning disable CS8625
public class TrivialTest
{
    #region Equality

    [Fact]
    public void TestEquality()
    {
        var rawValues = new decimal[]
        {
            0m,
            1m,
            2m,
            10m,
            1.0001m,
            1.00001m,
            1.00000000000001m,
            0.0001m,
            0.00001m,
            0.00000000000001m,
        };
        rawValues = rawValues
            .Concat(rawValues.Select(t => -t))
            .Distinct()
            .OrderBy(t => t)
            .ToArray();

        for (var i = 0; i < rawValues.Length; i++)
        {
            var operand1 = new BigDec(rawValues[i]);

            for (var j = 0; j < rawValues.Length; j++)
            {
                var operand2 = new BigDec(rawValues[j]);

                if (i == j)
                {
                    Assert.Equal(operand1, operand2);
                    Assert.Equal(operand2, operand1);
                    Assert.True(operand1 == operand2);
                    Assert.False(operand1 != operand2);
                    Assert.True(operand2 == operand1);
                    Assert.False(operand2 != operand1);
                }
                else
                {
                    Assert.NotEqual(operand1, operand2);
                    Assert.NotEqual(operand2, operand1);
                    Assert.False(operand1 == operand2);
                    Assert.True(operand1 != operand2);
                    Assert.False(operand2 == operand1);
                    Assert.True(operand2 != operand1);
                }
            }
        }
    }

    [Fact]
    [SuppressMessage("ReSharper", "VariableCanBeNotNullable")]
    public void TestEquality_null()
    {
        var a1 = BigDec.One;
        var b1 = BigDec.One;

        BigDec? c1a = BigDec.One;
        BigDec? d1a = BigDec.One;

        BigDec? c1b = null;
        BigDec? d1b = null;

        Assert.True(a1 == b1);
        Assert.False(a1 != b1);

        Assert.True(a1 == c1a);
        Assert.True(c1a == a1);
        Assert.True(c1a == d1a);
        Assert.False(a1 != c1a);
        Assert.False(c1a != a1);
        Assert.False(c1a != d1a);

        Assert.True(c1a != c1b);
        Assert.True(c1b != c1a);
        Assert.False(c1a == c1b);
        Assert.False(c1b == c1a);

        Assert.True(c1b == d1b);
        Assert.False(c1b != d1b);
    }

    #endregion

    #region Division

    public static object[][] TestDivisionData()
    {
        var operands1 = new decimal[]
        {
            0.0001m,
            0.00001m,
            1m,
            2m,
            10m,
            120m,
            0.1m,
            14.88m,
            0.100_000_01m,
            0.100_000_001m,
            1.000_000_001m,
            1.100_000_001m,
            1.000_000_000_001m,
            1.100_000_000_001m,
        };
        operands1 = operands1
            .Concat(operands1.Select(t => -t))
            .Distinct()
            .OrderBy(Math.Abs)
            .ToArray();

        var operands2 = new decimal[]
        {
            1m,
            2m,
            3m,
            4m,
            8m,
            16m,
            5m,
            50m,
            0.1m,
            0.0001m,
            0.00001m,
        };
        operands2 = operands2
            .Concat(operands2.Select(t => -t))
            .Distinct()
            .OrderBy(Math.Abs)
            .ToArray();

        var result = operands1
            .SelectMany(operand1 => operands2
                .Select(operand2 => (operand1, operand2)))
            .Where(x =>
            {
                var exp1 = ExpUp(x.operand1);
                var exp2 = ExpUp(x.operand2);

                return (exp1 * 1_000_000m % exp2 == 0);
            })
            .Select(t => new object[]
            {
                new BigDec(t.operand1),
                new BigDec(t.operand2),
                t.operand1 / t.operand2,
            })
            .ToArray();

        return result;
    }

    private static decimal ExpUp(decimal rawValue)
    {
        while (rawValue != Math.Floor(rawValue))
        {
            rawValue *= 10m;
        }

        return rawValue;
    }

    [Theory]
    [MemberData(nameof(TestDivisionData))]
    public void TestDivision(BigDec a, BigDec b, decimal expected)
    {
        var actual = a / b;

        Assert.Equal(new BigDec(expected), actual);
        Assert.True(actual == expected);
        Assert.True(expected == actual);
        Assert.False(actual != expected);
        Assert.False(expected != actual);
    }

    public static object[][] CheckDivisionsData()
    {
        var values = new decimal[]
        {
            1.0000000001m,
            1.123_456m,
            0.893_784_5m,
            1.787_569m,
        };

        return values
            .SelectMany(value1 => values
                .Where(x => x != value1)
                .Select(value2 => new object[] { value1, value2 }))
            .ToArray();
    }

    [Theory]
    [MemberData(nameof(CheckDivisionsData))]
    public void CheckDivisions(decimal value1, decimal value2)
    {
        var precisions = new int[] { 18, 36 };
        foreach (var precision1 in precisions)
        {
            foreach (var precision2 in precisions)
            {
                CheckDivisions_Inner(value1, value2, precision1, precision2);
            }
        }
    }

    private void CheckDivisions_Inner(decimal value1, decimal value2, int precision1, int precision2)
    {
        var v1 = new BigDec(value1).WithPrecision(precision1);
        var v2 = new BigDec(value2).WithPrecision(precision2);

        var expected = value1 / value2;
        var actual = v1 / v2;

        var diff = (actual - expected).Abs();
        Assert.True(diff <= 0.000_000_000_01m);
    }

    [Fact]
    public void InfiniteReducing()
    {
        var composites = new (decimal operandA, decimal operandB)[]
        {
            (1, 3),
            (1, 17),
            (7, 44),
        };

        var stepSize = BigInteger.Pow(10, 3);

        foreach (var (operandA, operandB) in composites)
        {
            var operandA1 = new BigDec(operandA);
            var operandB1 = new BigDec(operandB);

            var operandA2 = operandA1.WithPrecision(50);
            var operandB2 = operandB1.WithPrecision(50);
            var actual = operandA2 / operandB2;

            for (var i = 3; i < 36; i += 3)
            {
                operandA2 /= stepSize;
                operandB2 /= stepSize;

                var expected = operandA2 / operandB2;
                var diff = actual - expected;
                Assert.True(diff <= 0.000_000_000_01m);
            }
        }
    }

    #region Additional division cases

    public static object[][] AdditionalDivisionCases_TheoryData()
    {
        var denominator_tail = BigDec.One / BigInteger.Pow(BigDec.BigInteger10, 18);
        var denominators = new string[]
        {
            "207161.63281935526273129",
            "1",
            "2.5",
            "10",
            "0.1",
            "0.001",
        };

        return denominators
            .Select(denominator =>
            {
                var t = BigDec.Parse(denominator);
                return new BigDec[] { t, t + denominator_tail };
            })
            .SelectMany(ar => ar.Cast<object>().Select(t => new object[] { t }))
            .ToArray();
    }

    [Theory]
    [MemberData(nameof(AdditionalDivisionCases_TheoryData))]
    public void AdditionalDivisionCases_Theory(BigDec denominator)
    {
        var denominatorDouble = (double)denominator;
        var correctionCoef = BigInteger.One;
        var expected0 = BigDec.One / denominator;
        for (var i = 0; i < 36; i++)
        {
            var nominator = BigDec.One.WithPrecision(18 + i) / correctionCoef;
            var nominatorDouble = Math.Pow(0.1d, i);
            Assert.True(nominator > BigDec.Zero);

            var division = nominator / denominator;
            Assert.True(division > BigDec.Zero);

            if (i < 10)
            {
                var expectedValue = nominatorDouble / denominatorDouble;
                var actualDouble = (double)division;

                Assert.InRange(actualDouble, expectedValue * 0.999d, expectedValue * 1.001d);
            }

            {
                var divisionCorrected = division * correctionCoef;
                Assert.InRange(divisionCorrected, expected0 * 0.999_999_999m, expected0 * 1.000_000_001m);
                var rCoef = (divisionCorrected > expected0)
                    ? divisionCorrected.WithPrecision(1000 + i) / expected0
                    : expected0 / divisionCorrected.WithPrecision(1000 + i);
                rCoef -= 1;
                Assert.InRange(rCoef, BigDec.Zero, new BigDec(0.000_000_000_001m));
            }

            correctionCoef *= BigDec.BigInteger10;
        }
    }

    #endregion

    #endregion

    public static ICollection<object[]> CheckLongTailData()
    {
        var result = new List<object[]>();

        for (var exp1 = -30; exp1 <= 30; exp1++)
        {
            if (exp1 < 0)
            {
                var operand1 = BigDec.One / BigInteger.Pow(BigDec.BigInteger10, -exp1);
                if (operand1.IsZero)
                {
                    continue;
                }
            }

            for (var exp2 = -30; exp2 <= 30; exp2++)
            {
                if (exp2 < 0)
                {
                    var operand2 = BigDec.One / BigInteger.Pow(BigDec.BigInteger10, -exp2);
                    if (operand2.IsZero)
                    {
                        continue;
                    }
                }

                result.Add(new object[] { exp1, exp2 });
            }
        }

        return result;
    }

    [Theory]
    [MemberData(nameof(CheckLongTailData))]
    public void CheckLongTail(int exp1, int exp2)
    {
        BigDec operand1 = BigDec.One;
        if (exp1 >= 0)
        {
            operand1 *= BigInteger.Pow(BigDec.BigInteger10, exp1);
        }
        else
        {
            operand1 /= BigInteger.Pow(BigDec.BigInteger10, -exp1);
        }

        BigDec operand2 = BigDec.One;
        if (exp2 >= 0)
        {
            operand2 *= BigInteger.Pow(BigDec.BigInteger10, exp2);
        }
        else
        {
            operand2 /= BigInteger.Pow(BigDec.BigInteger10, -exp2);
        }

        var result = operand1 / operand2;
        var expected = exp1 - exp2;
        if (-expected > result.MaxPrecision)
        {
            result = result.Round(result.MaxPrecision);
            Assert.Equal(BigDec.Zero, result);
            return;
        }

        var actual = 0;

        var _value = result.Mantissa;
        while (_value >= 10)
        {
            result /= 10;
            _value = result.Mantissa;
            actual++;
        }

        Assert.Equal(BigInteger.One, _value);
        actual -= result.Offset;

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void CheckLongTailAdd()
    {
        var a = new BigDec(1m, 5);
        var b = new BigDec(1m, 30);
        b /= BigInteger.Pow(new BigInteger(10), 20);

        var c = a / b;
        var actual = 0;
        while (c >= new BigDec(10))
        {
            actual++;
            c /= new BigDec(10);
        }

        Assert.Equal(BigDec.One, c);
        Assert.Equal(20, actual);
    }

    [Fact]
    public void ConstructorDoubleInfinite()
    {
        foreach (var value in new double[]
                 {
                     double.NegativeInfinity,
                     double.PositiveInfinity,
                     double.NaN,
                 })
        {
            Assert.Throws<InfiniteDecimalException>(() => { _ = new BigDec(value); });
        }
    }

    [Fact]
    public void ConstructorFloatInfinite()
    {
        foreach (var value in new float[]
                 {
                     float.NegativeInfinity,
                     float.PositiveInfinity,
                     float.NaN,
                 })
        {
            Assert.Throws<InfiniteDecimalException>(() => { _ = new BigDec(value); });
        }
    }

    #region inequality

    public static object[][] CheckInequalityData()
    {
        var rawValues = new decimal[]
        {
            0m,
            1m,
            2m,
            10m,
            1.0001m,
            1.00001m,
            1.00000000000001m,
            0.0001m,
            0.00001m,
            0.00000000000001m,
            sbyte.MaxValue,
            byte.MaxValue,
            ushort.MaxValue,
            short.MaxValue,
            3 * ushort.MaxValue,
        };
        rawValues = rawValues
            .Concat(rawValues.Select(t => -t))
            .Distinct()
            .OrderBy(t => t)
            .ToArray();

        var result = new List<object[]>();
        foreach (var operand1 in rawValues)
        {
            var a = new BigDec(operand1);

            foreach (var operand2 in rawValues)
            {
                var b = new BigDec(operand2);

                int expected;
                if (operand1 == operand2)
                {
                    expected = 0;
                }
                else if (operand1 > operand2)
                {
                    expected = 1;
                }
                else
                {
                    expected = -1;
                }

                result.Add(new object[] { a, b, expected });
            }
        }

        return result.ToArray();
    }

    [Theory]
    [MemberData(nameof(CheckInequalityData))]
    [SuppressMessage("ReSharper", "ConvertIfStatementToSwitchStatement")]
    public void CheckInequality(
        BigDec operand1,
        BigDec operand2,
        int expectedEquality
    )
    {
        if (expectedEquality == 0)
        {
            Assert.Equal(operand1, operand2);
            Assert.Equal(operand2, operand1);
            Assert.True(operand1 == operand2);
            Assert.True(operand2 == operand1);
            Assert.False(operand1 != operand2);
            Assert.False(operand2 != operand1);

            Assert.True(operand1 >= operand2);
            Assert.True(operand1 <= operand2);
            Assert.True(operand2 >= operand1);
            Assert.True(operand2 <= operand1);

            Assert.False(operand1 > operand2);
            Assert.False(operand1 < operand2);
            Assert.False(operand2 > operand1);
            Assert.False(operand2 < operand1);

            var setSingle = new HashSet<BigDec>() { operand1, operand2 };
            Assert.Single(setSingle);

            return;
        }

        if (expectedEquality == -1)
        {
            (operand2, operand1) = (operand1, operand2);
            expectedEquality = 1;
        }

        if (expectedEquality != 1)
        {
            throw new ArgumentOutOfRangeException(nameof(expectedEquality));
        }

        CheckInequality_inner(operand1, operand2);
    }

    private void CheckInequality_inner(
        BigDec operand1,
        BigDec operand2
    )
    {
        Assert.True(operand1 > operand2);
        Assert.True(operand1 >= operand2);
        Assert.False(operand1 < operand2);
        Assert.False(operand1 <= operand2);
        Assert.False(operand1 == operand2);
        Assert.False(operand2 == operand1);
        Assert.True(operand1 != operand2);
        Assert.True(operand2 != operand1);
        Assert.Equal(1, operand1.CompareTo(operand2));
        Assert.Equal(-1, operand2.CompareTo(operand1));

        Assert.True(operand1 > (decimal)operand2);
        Assert.True(operand1 >= (decimal)operand2);
        Assert.True((decimal)operand1 > operand2);
        Assert.True((decimal)operand1 >= operand2);
        Assert.False(operand1 < (decimal)operand2);
        Assert.False(operand1 <= (decimal)operand2);
        Assert.False((decimal)operand1 < operand2);
        Assert.False((decimal)operand1 <= operand2);
        Assert.False((decimal)operand1 == operand2);
        Assert.True((decimal)operand1 != operand2);
        Assert.False(operand1 == (decimal)operand2);
        Assert.True(operand1 != (decimal)operand2);
        Assert.Equal(1, operand1.CompareTo((decimal)operand2));
        Assert.Equal(-1, operand2.CompareTo((decimal)operand1));

        Assert.True(operand1 > (double)operand2);
        Assert.True(operand1 >= (double)operand2);
        Assert.True((double)operand1 > operand2);
        Assert.True((double)operand1 >= operand2);
        Assert.False(operand1 < (double)operand2);
        Assert.False(operand1 <= (double)operand2);
        Assert.False((double)operand1 < operand2);
        Assert.False((double)operand1 <= operand2);
        Assert.False((double)operand1 == operand2);
        Assert.True((double)operand1 != operand2);
        Assert.False(operand1 == (double)operand2);
        Assert.True(operand1 != (double)operand2);
        Assert.Equal(1, operand1.CompareTo((double)operand2));
        Assert.Equal(-1, operand2.CompareTo((double)operand1));

        {
            var value1 = operand1.Mantissa;
            var value2 = operand2.Mantissa;
            var lg10V1 = BigInteger.Log10(BigInteger.Abs(value1));
            var lg10V2 = BigInteger.Log10(BigInteger.Abs(value2));
            // log10(2) * 23 ~= 6.923
            if ((lg10V1 >= 6.923) || (lg10V2 >= 6.923))
            {
                // float can't hold such values
            }
            else
            {
                Assert.True(operand1 > (float)operand2);
                Assert.True(operand1 >= (float)operand2);
                Assert.True((float)operand1 > operand2);
                Assert.True((float)operand1 >= operand2);
                Assert.False(operand1 < (float)operand2);
                Assert.False(operand1 <= (float)operand2);
                Assert.False((float)operand1 < operand2);
                Assert.False((float)operand1 <= operand2);
                Assert.False((float)operand1 == operand2);
                Assert.True((float)operand1 != operand2);
                Assert.False(operand1 == (float)operand2);
                Assert.True(operand1 != (float)operand2);
                Assert.Equal(1, operand1.CompareTo((float)operand2));
                Assert.Equal(-1, operand2.CompareTo((float)operand1));
            }
        }

        if (operand1.IsInteger)
        {
            Assert.True((BigInteger)operand1 > operand2);
            Assert.True((BigInteger)operand1 >= operand2);
            Assert.Equal(-1, operand2.CompareTo((BigInteger)operand1));
            Assert.True((long)operand1 > operand2);
            Assert.True((long)operand1 >= operand2);
            Assert.Equal(-1, operand2.CompareTo((long)operand1));
            Assert.True((int)operand1 > operand2);
            Assert.True((int)operand1 >= operand2);
            Assert.Equal(-1, operand2.CompareTo((int)operand1));
            Assert.False((BigInteger)operand1 < operand2);
            Assert.False((BigInteger)operand1 <= operand2);
            Assert.False((long)operand1 < operand2);
            Assert.False((long)operand1 <= operand2);
            Assert.False((int)operand1 < operand2);
            Assert.False((int)operand1 <= operand2);
            Assert.False((long)operand1 == operand2);
            Assert.True((long)operand1 != operand2);
            Assert.False((int)operand1 == operand2);
            Assert.True((int)operand1 != operand2);
            Assert.False((BigInteger)operand1 == operand2);
            Assert.True((BigInteger)operand1 != operand2);
            Assert.False((BigInteger)operand1 == operand2);
            Assert.True((BigInteger)operand1 != operand2);

            if ((operand1 >= ulong.MinValue) && (operand1 <= ulong.MaxValue))
            {
                Assert.True(operand1.ToUInt64(null) > operand2);
                Assert.True(operand1.ToUInt64(null) >= operand2);
                Assert.False(operand1.ToUInt64(null) < operand2);
                Assert.False(operand1.ToUInt64(null) <= operand2);
                Assert.False(operand1.ToUInt64(null) == operand2);
                Assert.True(operand1.ToUInt64(null) != operand2);
                Assert.Equal(-1, operand2.CompareTo(operand1.ToUInt64(null)));
            }

            if ((operand1 >= uint.MinValue) && (operand1 <= uint.MaxValue))
            {
                Assert.True(operand1.ToUInt32(null) > operand2);
                Assert.True(operand1.ToUInt32(null) >= operand2);
                Assert.False(operand1.ToUInt32(null) < operand2);
                Assert.False(operand1.ToUInt32(null) <= operand2);
                Assert.False(operand1.ToUInt32(null) == operand2);
                Assert.True(operand1.ToUInt32(null) != operand2);
                Assert.Equal(-1, operand2.CompareTo(operand1.ToUInt32(null)));
            }

            if ((operand1 >= ushort.MinValue) && (operand1 <= ushort.MaxValue))
            {
                Assert.True(operand1.ToUInt16(null) > operand2);
                Assert.True(operand1.ToUInt16(null) >= operand2);
                Assert.False(operand1.ToUInt16(null) < operand2);
                Assert.False(operand1.ToUInt16(null) <= operand2);
                Assert.False(operand1.ToUInt16(null) == operand2);
                Assert.True(operand1.ToUInt16(null) != operand2);
                Assert.Equal(-1, operand2.CompareTo(operand1.ToUInt16(null)));
            }

            if ((operand1 >= short.MinValue) && (operand1 <= short.MaxValue))
            {
                Assert.True(operand1.ToInt16(null) > operand2);
                Assert.True(operand1.ToInt16(null) >= operand2);
                Assert.False(operand1.ToInt16(null) < operand2);
                Assert.False(operand1.ToInt16(null) <= operand2);
                Assert.False(operand1.ToInt16(null) == operand2);
                Assert.True(operand1.ToInt16(null) != operand2);
                Assert.Equal(-1, operand2.CompareTo(operand1.ToInt16(null)));
            }

            if ((operand1 >= byte.MinValue) && (operand1 <= byte.MaxValue))
            {
                Assert.True(operand1.ToByte(null) > operand2);
                Assert.True(operand1.ToByte(null) >= operand2);
                Assert.False(operand1.ToByte(null) < operand2);
                Assert.False(operand1.ToByte(null) <= operand2);
                Assert.False(operand1.ToByte(null) == operand2);
                Assert.True(operand1.ToByte(null) != operand2);
                Assert.Equal(-1, operand2.CompareTo(operand1.ToByte(null)));
            }

            if ((operand1 >= sbyte.MinValue) && (operand1 <= sbyte.MaxValue))
            {
                Assert.True((sbyte)operand1 > operand2);
                Assert.True((sbyte)operand1 >= operand2);
                Assert.False((sbyte)operand1 < operand2);
                Assert.False((sbyte)operand1 <= operand2);
                Assert.False((sbyte)operand1 == operand2);
                Assert.True((sbyte)operand1 != operand2);
                Assert.Equal(-1, operand2.CompareTo((sbyte)operand1));
            }
        }

        if (operand2.IsInteger)
        {
            Assert.True(operand1 > (BigInteger)operand2);
            Assert.True(operand1 >= (BigInteger)operand2);
            Assert.Equal(1, operand1.CompareTo((BigInteger)operand2));
            Assert.True(operand1 > (long)operand2);
            Assert.True(operand1 >= (long)operand2);
            Assert.Equal(1, operand1.CompareTo((long)operand2));
            Assert.True(operand1 > (int)operand2);
            Assert.True(operand1 >= (int)operand2);
            Assert.Equal(1, operand1.CompareTo((int)operand2));
            Assert.False(operand1 < (BigInteger)operand2);
            Assert.False(operand1 <= (BigInteger)operand2);
            Assert.False(operand1 < (long)operand2);
            Assert.False(operand1 <= (long)operand2);
            Assert.False(operand1 < (int)operand2);
            Assert.False(operand1 <= (int)operand2);
            Assert.False(operand1 == (long)operand2);
            Assert.True(operand1 != (long)operand2);
            Assert.False(operand1 == (int)operand2);
            Assert.True(operand1 != (int)operand2);
            Assert.False(operand1 == (BigInteger)operand2);
            Assert.True(operand1 != (BigInteger)operand2);
            Assert.False(operand1 == (BigInteger)operand2);
            Assert.True(operand1 != (BigInteger)operand2);

            if ((operand2 >= ulong.MinValue) && (operand2 <= ulong.MaxValue))
            {
                Assert.True(operand1 > operand2.ToUInt64(null));
                Assert.True(operand1 >= operand2.ToUInt64(null));
                Assert.False(operand1 < operand2.ToUInt64(null));
                Assert.False(operand1 <= operand2.ToUInt64(null));
                Assert.False(operand1 == operand2.ToUInt64(null));
                Assert.True(operand1 != operand2.ToUInt64(null));
                Assert.Equal(1, operand1.CompareTo(operand2.ToUInt64(null)));
            }

            if ((operand2 >= uint.MinValue) && (operand2 <= uint.MaxValue))
            {
                Assert.True(operand1 > operand2.ToUInt32(null));
                Assert.True(operand1 >= operand2.ToUInt32(null));
                Assert.False(operand1 < operand2.ToUInt32(null));
                Assert.False(operand1 <= operand2.ToUInt32(null));
                Assert.False(operand1 == operand2.ToUInt32(null));
                Assert.True(operand1 != operand2.ToUInt32(null));
                Assert.Equal(1, operand1.CompareTo(operand2.ToUInt32(null)));
            }

            if ((operand2 >= ushort.MinValue) && (operand2 <= ushort.MaxValue))
            {
                Assert.True(operand1 > operand2.ToUInt16(null));
                Assert.True(operand1 >= operand2.ToUInt16(null));
                Assert.False(operand1 < operand2.ToUInt16(null));
                Assert.False(operand1 <= operand2.ToUInt16(null));
                Assert.False(operand1 == operand2.ToUInt16(null));
                Assert.True(operand1 != operand2.ToUInt16(null));
                Assert.Equal(1, operand1.CompareTo(operand2.ToUInt16(null)));
            }

            if ((operand2 >= short.MinValue) && (operand2 <= short.MaxValue))
            {
                Assert.True(operand1 > operand2.ToInt16(null));
                Assert.True(operand1 >= operand2.ToInt16(null));
                Assert.False(operand1 < operand2.ToInt16(null));
                Assert.False(operand1 <= operand2.ToInt16(null));
                Assert.False(operand1 == operand2.ToInt16(null));
                Assert.True(operand1 != operand2.ToInt16(null));
                Assert.Equal(1, operand1.CompareTo(operand2.ToInt16(null)));
            }

            if ((operand2 >= byte.MinValue) && (operand2 <= byte.MaxValue))
            {
                Assert.True(operand1 > operand2.ToByte(null));
                Assert.True(operand1 >= operand2.ToByte(null));
                Assert.False(operand1 < operand2.ToByte(null));
                Assert.False(operand1 <= operand2.ToByte(null));
                Assert.False(operand1 == operand2.ToByte(null));
                Assert.True(operand1 != operand2.ToByte(null));
                Assert.Equal(1, operand1.CompareTo(operand2.ToByte(null)));
            }

            if ((operand2 >= sbyte.MinValue) && (operand2 <= sbyte.MaxValue))
            {
                Assert.True(operand1 > (sbyte)operand2);
                Assert.True(operand1 >= (sbyte)operand2);
                Assert.False(operand1 < (sbyte)operand2);
                Assert.False(operand1 <= (sbyte)operand2);
                Assert.False(operand1 == (sbyte)operand2);
                Assert.True(operand1 != (sbyte)operand2);
                Assert.Equal(1, operand1.CompareTo((sbyte)operand2));
            }
        }

        Assert.NotEqual(operand1, operand2);
        Assert.NotEqual(operand2, operand1);

        var set = new HashSet<BigDec>() { operand1, operand2 };
        Assert.Equal(2, set.Count);
    }

    [Fact]
    public void CompareWithNull()
    {
        int expected = 10.CompareTo(null);

        foreach (var value in new decimal[] { -1, 0, 1 })
        {
            var a = new BigDec(value);
            Assert.Equal(expected, a.CompareTo(null));
        }
    }

    [Fact]
    public void CompareWithUnknownType()
    {
        Assert.Throws<InfiniteDecimalException>(() => { _ = BigDec.Zero.CompareTo(string.Empty); });
    }

    #endregion

    #region max precision

    public static object[][] CheckPrecisionData()
    {
        var rawValues = new decimal[]
        {
            0m,
            1m,
            2m,
            10m,
            1.0001m,
            1.00001m,
            1.00000000000001m,
            0.0001m,
            0.00001m,
            0.00000000000001m,
        };
        rawValues = rawValues
            .Concat(rawValues.Select(t => -t))
            .Distinct()
            .OrderBy(t => t)
            .ToArray();

        return rawValues
            .SelectMany(operand1 => Enumerable
                .Range(10, 20)
                .Where(x => x % 3 == 0)
                .Where(x => x != 18)
                .SelectMany(operand1Prec => rawValues
                    .Select(operand2 => new object[]
                    {
                        new BigDec(operand1, operand1Prec),
                        new BigDec(operand2),
                        operand1Prec,
                    })
                ))
            .ToArray();
    }

    [Theory]
    [MemberData(nameof(CheckPrecisionData))]
    public void CheckPrecision(
        BigDec operand1,
        BigDec operand2,
        int _
    )
    {
        var minPrecision = Math.Max(operand1.MaxPrecision, operand2.MaxPrecision);
        var result = operand1 - operand2;
        Assert.InRange(result.MaxPrecision, minPrecision, int.MaxValue);

        result = operand1 + operand2;
        Assert.InRange(result.MaxPrecision, minPrecision, int.MaxValue);

        result = operand2 - operand1;
        Assert.InRange(result.MaxPrecision, minPrecision, int.MaxValue);

        result = -operand1;
        Assert.InRange(result.MaxPrecision, operand1.MaxPrecision, int.MaxValue);

        result = operand1 * operand2;
        Assert.InRange(result.MaxPrecision, minPrecision, int.MaxValue);

        result = operand2 * operand1;
        Assert.InRange(result.MaxPrecision, minPrecision, int.MaxValue);

        if (operand2 > BigDec.Zero)
        {
            result = operand1 / operand2;
            if ((result != BigDec.One) && (result != BigDec.Zero))
            {
                Assert.InRange(result.MaxPrecision, minPrecision, int.MaxValue);
            }
        }

        // ReSharper disable once InvertIf
        if (operand1 > BigDec.Zero)
        {
            result = operand2 / operand1;
            if ((result != BigDec.One) && (result != BigDec.Zero))
            {
                Assert.InRange(result.MaxPrecision, minPrecision, int.MaxValue);
            }
        }
    }

    #endregion

    #region exceptions

    [Fact]
    public void ParseMultiDotString()
    {
        Assert.Throws<InfiniteDecimalException>(() => { BigDec.Parse("123.321.11"); });
    }

    [Fact]
    public void DivisionByZero()
    {
        Assert.Throws<InfiniteDecimalException>(() =>
        {
            var _ = BigDec.One / BigDec.Zero;
        });
        Assert.Throws<InfiniteDecimalException>(() =>
        {
            var _ = BigDec.One / BigInteger.Zero;
        });
        Assert.Throws<InfiniteDecimalException>(() =>
        {
            var _ = BigDec.Zero / BigDec.Zero;
        });
        Assert.Throws<InfiniteDecimalException>(() =>
        {
            var _ = BigDec.Zero / BigInteger.Zero;
        });
        Assert.Throws<InfiniteDecimalException>(() =>
        {
            var _ = BigDec.One / (decimal)0;
        });
        Assert.Throws<InfiniteDecimalException>(() =>
        {
            var _ = BigDec.One / (double)0;
        });
        Assert.Throws<InfiniteDecimalException>(() =>
        {
            var _ = BigDec.One / (float)0;
        });

        //
        Assert.Throws<InfiniteDecimalException>(() =>
        {
            var _ = BigDec.One / (byte)0;
        });
        Assert.Throws<InfiniteDecimalException>(() =>
        {
            var _ = BigDec.One / (sbyte)0;
        });
        Assert.Throws<InfiniteDecimalException>(() =>
        {
            var _ = BigDec.One / (ushort)0;
        });
        Assert.Throws<InfiniteDecimalException>(() =>
        {
            var _ = BigDec.One / (short)0;
        });
        Assert.Throws<InfiniteDecimalException>(() =>
        {
            var _ = BigDec.One / (uint)0;
        });
        Assert.Throws<InfiniteDecimalException>(() =>
        {
            // ReSharper disable once RedundantCast
            var _ = BigDec.One / (int)0;
        });
        Assert.Throws<InfiniteDecimalException>(() =>
        {
            var _ = BigDec.One / (ulong)0;
        });
        Assert.Throws<InfiniteDecimalException>(() =>
        {
            var _ = BigDec.One / (long)0;
        });
    }

    [Fact]
    public void ToChar()
    {
        Assert.Throws<InfiniteDecimalException>(() =>
        {
            var _ = BigDec.One.ToChar(null);
        });
    }

    [Fact]
    public void ToDateTime()
    {
        Assert.Throws<InfiniteDecimalException>(() =>
        {
            var _ = BigDec.One.ToDateTime(null);
        });
    }

    [Fact]
    public void NegativeOffset()
    {
        var offsetField = typeof(BigDec)
            .GetProperty("Offset", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
        if (offsetField is null)
        {
            throw new Exception("'Offset' property does not exist");
        }

        var bio = new BigDec(-2);

        try
        {
            offsetField.SetValue(bio, -2);
        }
        catch (TargetInvocationException e)
        {
            Assert.NotNull(e.InnerException);
            Assert.Equal(typeof(InfiniteDecimalException), e.InnerException.GetType());
        }
    }

    [Fact]
    public void UnreachableTypeForCasting()
    {
        Assert.Throws<InfiniteDecimalException>(() =>
        {
            var _ = BigDec.One.ToType(typeof(Task), null);
        });
    }

    #endregion

    #region different small tests

    [Fact]
    public void EqualWithNullViaOperator()
    {
        Assert.False(BigDec.One == null);
        Assert.True(BigDec.One != null);
        Assert.False(null == BigDec.One);
        Assert.True(null != BigDec.One);
    }

    [Fact]
    public void EqualWithNull()
    {
        Assert.False(BigDec.One.Equals(null));
    }

    [Fact]
    public void EqualWithBigInt()
    {
        // ReSharper disable once SuspiciousTypeConversion.Global
        Assert.False(BigDec.One.Equals(BigInteger.One));
    }

    [Fact]
    public void IsTypeCodeAnObject()
    {
        Assert.Equal(TypeCode.Object, BigDec.One.GetTypeCode());
    }

    #endregion

    #region conversion long/ulong

    [SuppressMessage("ReSharper", "RedundantCast")]
    public static object[][] TestConversionUlongData()
    {
        var values = new ulong[]
        {
            0,
            (ulong)sbyte.MaxValue - 1,
            (ulong)sbyte.MaxValue,
            (ulong)sbyte.MaxValue + 1,
            byte.MaxValue - (ulong)1,
            byte.MaxValue,
            byte.MaxValue + (ulong)1,
            (ulong)short.MaxValue - 1,
            (ulong)short.MaxValue,
            (ulong)short.MaxValue + 1,
            ushort.MaxValue - (ulong)1,
            ushort.MaxValue,
            ushort.MaxValue + (ulong)1,
            (ulong)int.MaxValue - 1,
            (ulong)int.MaxValue,
            (ulong)int.MaxValue + 1,
            uint.MaxValue - 1,
            uint.MaxValue,
            uint.MaxValue + (ulong)1,
            (ulong)long.MaxValue - 1,
            (ulong)long.MaxValue,
            (ulong)long.MaxValue + 1,
            ulong.MaxValue - 1,
            ulong.MaxValue,
        };

        return values.Select(t => new object[] { t }).ToArray();
    }

    [Theory]
    [MemberData(nameof(TestConversionUlongData))]
    [SuppressMessage("ReSharper", "InvertIf")]
    public void TestConversionUlong(ulong value)
    {
        var bio = new BigDec(value);
        Assert.True(value == bio);
        Assert.False(value != bio);

        var reUlong = bio.ToUInt64(null);
        Assert.Equal(value, reUlong);
        reUlong = (ulong)bio.ToType(typeof(ulong), null);
        Assert.Equal(value, reUlong);
        reUlong = (ulong)bio;
        Assert.Equal(value, reUlong);

        if (value <= uint.MaxValue)
        {
            uint valueCasted = (uint)value;
            uint reUint = bio.ToUInt32(null);
            Assert.Equal(valueCasted, reUint);
            reUint = (uint)bio.ToType(typeof(uint), null);
            Assert.Equal(valueCasted, reUint);
        }

        if (value <= ushort.MaxValue)
        {
            ushort valueCasted = (ushort)value;
            ushort reUshort = bio.ToUInt16(null);
            Assert.Equal(valueCasted, reUshort);
            reUshort = (ushort)bio.ToType(typeof(ushort), null);
            Assert.Equal(valueCasted, reUshort);
        }

        if (value <= byte.MaxValue)
        {
            byte valueCasted = (byte)value;
            byte reByte = bio.ToByte(null);
            Assert.Equal(valueCasted, reByte);
            reByte = (byte)bio.ToType(typeof(byte), null);
            Assert.Equal(valueCasted, reByte);
        }
    }

    public static object[][] TestConversionLongData()
    {
        var values = new long[]
        {
            0,
            sbyte.MaxValue - (long)1,
            sbyte.MaxValue,
            sbyte.MaxValue + (long)1,
            short.MaxValue - (long)1,
            short.MaxValue,
            short.MaxValue + (long)1,
            int.MaxValue - 1,
            int.MaxValue,
            int.MaxValue + (long)1,
            long.MaxValue - 1,
            long.MaxValue,
        };

        return values.Select(t => new object[] { t }).ToArray();
    }

    [Theory]
    [MemberData(nameof(TestConversionLongData))]
    [SuppressMessage("ReSharper", "InvertIf")]
    public void TestConversionLong(long value)
    {
        var bio = new BigDec(value);
        Assert.True(value == bio);
        Assert.False(value != bio);

        var reLong = bio.ToInt64(null);
        Assert.Equal(value, reLong);
        reLong = (long)bio.ToType(typeof(long), null);
        Assert.Equal(value, reLong);
        reLong = (long)bio;
        Assert.Equal(value, reLong);

        if (value is >= int.MinValue and <= int.MaxValue)
        {
            int valueCasted = (int)value;
            int reInt = bio.ToInt32(null);
            Assert.Equal(valueCasted, reInt);
            reInt = (int)bio.ToType(typeof(int), null);
            Assert.Equal(valueCasted, reInt);
        }

        if (value is >= short.MinValue and <= short.MaxValue)
        {
            short valueCasted = (short)value;
            short reShort = bio.ToInt16(null);
            Assert.Equal(valueCasted, reShort);
            reShort = (short)bio.ToType(typeof(short), null);
            Assert.Equal(valueCasted, reShort);
        }

        if (value is >= sbyte.MinValue and <= sbyte.MaxValue)
        {
            sbyte valueCasted = (sbyte)value;
            sbyte reSbyte = bio.ToSByte(null);
            Assert.Equal(valueCasted, reSbyte);
            reSbyte = (sbyte)bio.ToType(typeof(sbyte), null);
            Assert.Equal(valueCasted, reSbyte);
        }
    }

    [Fact]
    public void ToBooleanTest()
    {
        var values = new decimal[]
        {
            -10m,
            -1m,
            -0.5m,
            0,
            0.5m,
            1m,
            10m,
        };

        foreach (var value in values)
        {
            var u = (value > 0m);
            var bio = new BigDec(value);
            Assert.Equal(u, bio.ToBoolean(null));
        }
    }

    #endregion

    #region Culture ToString

    public static object?[][] TestCultureData()
    {
        var russiaCulture = CultureInfo.GetCultureInfo("ru-RU");
        var usaCulture = CultureInfo.GetCultureInfo("en-US");

        var items = new (decimal value, CultureInfo? cultureInfo, string expectedString)[]
        {
            // (1.1m, null, "1.1"),
            (1.1m, CultureInfo.InvariantCulture, "1.1"),
            (1.1m, usaCulture, "1.1"),
            (1.1m, russiaCulture, "1,1"),
            // (-1.1m, null, "-1.1"),
            (-1.1m, CultureInfo.InvariantCulture, "-1.1"),
            (-1.1m, usaCulture, "-1.1"),
            (-1.1m, russiaCulture, "-1,1"),
            // (1.00001m, null, "1.00001"),
            (1.00001m, CultureInfo.InvariantCulture, "1.00001"),
            (1.00001m, usaCulture, "1.00001"),
            (1.00001m, russiaCulture, "1,00001"),
        };

        return items
            .Select(t => new object?[] { t.value, t.cultureInfo, t.expectedString })
            .ToArray();
    }

    [Theory]
    [MemberData(nameof(TestCultureData))]
    public void TestCulture(decimal value, CultureInfo cultureInfo, string expectedString)
    {
        var decimalActual = value.ToString(cultureInfo);
        Assert.Equal(decimalActual, expectedString);

        //
        var bio = new BigDec(value);
        var actual1 = bio.ToString(cultureInfo);
        var actual2 = bio.ToStringDouble(cultureInfo);
        var actual3 = (string)bio.ToType(typeof(string), cultureInfo);

        Assert.Equal(expectedString, actual1);
        Assert.Equal(expectedString, actual2);
        Assert.Equal(expectedString, actual3);
    }

    #endregion

    #region BigInteger conversion

    [SuppressMessage("ReSharper", "LoopCanBeConvertedToQuery")]
    public static object[][] TestBigIntegerConversionData()
    {
        var items = new BigInteger[]
        {
            1,
            7,
            -7,
            31,
            -31,
            100_500,
            BigInteger.Parse("1234567890123456789"),
            BigInteger.Parse("12345678901234567890123456789012345678901234567890123456789012345678901234567890"),
            -BigInteger.Parse("12345678901234567890123456789012345678901234567890123456789012345678901234567890"),
        };

        var result = new List<object[]>();
        foreach (var hiddenArg in items)
        {
            foreach (var operand2 in items)
            {
                var operand1 = hiddenArg * operand2;
                result.Add(new object[] { operand1, operand1 });
            }
        }

        return result.ToArray();
    }

    [Theory]
    [MemberData(nameof(TestBigIntegerConversionData))]
    public void TestBigIntegerConversion(
        BigInteger operand1,
        BigInteger operand2
    )
    {
        var arg1 = new BigDec(operand1);
        var arg2 = new BigDec(operand2);

        Assert.Equal(operand1, (BigInteger)arg1);
        Assert.Equal(operand2, (BigInteger)arg2);
        Assert.Equal(operand1, (BigInteger)(arg1.ToType(typeof(BigInteger), null)));
        Assert.Equal(operand2, (BigInteger)(arg2.ToType(typeof(BigInteger), null)));

        Assert.True(arg1 == operand1);
        Assert.False(arg1 != operand1);
        Assert.True(arg2 == operand2);
        Assert.False(arg2 != operand2);

        var delimBI = operand1 / operand2;

        var delimResultBI = arg1 / arg2;
        var delimResultBIO = new BigDec(delimBI);
        Assert.Equal(delimResultBI, delimResultBIO);
        Assert.True(delimResultBIO == delimBI);
        Assert.True(delimResultBI == delimBI);
        Assert.False(delimResultBI != delimBI);

        delimResultBI = operand1 / arg2;
        Assert.Equal(delimResultBI, delimResultBIO);
        Assert.True(delimResultBIO == delimBI);
        Assert.True(delimResultBI == delimBI);
        Assert.False(delimResultBI != delimBI);

        delimResultBI = arg1 / operand2;
        Assert.Equal(delimResultBI, delimResultBIO);
        Assert.True(delimResultBIO == delimBI);
        Assert.True(delimResultBI == delimBI);
        Assert.False(delimResultBI != delimBI);
    }

    [SuppressMessage("ReSharper", "LoopCanBeConvertedToQuery")]
    public static object[][] TestBigInteger1ConversionData()
    {
        var items = new BigInteger[]
        {
            1,
            7,
            -7,
            31,
            -31,
            100_500,
            BigInteger.Parse("1234567890123456789"),
            BigInteger.Parse("12345678901234567890123456789012345678901234567890123456789012345678901234567890"),
            -BigInteger.Parse("12345678901234567890123456789012345678901234567890123456789012345678901234567890"),
        };

        return items.Select(t => new object[] { t }).ToArray();
    }

    [Theory]
    [MemberData(nameof(TestBigInteger1ConversionData))]
    public void TestBigInteger1Conversion(BigInteger operand)
    {
        var value = new BigDec(operand);
        var value01 = new BigDec(0.1m);

        Assert.True(value != 0.1m);
        Assert.False(value == 0.1m);
        Assert.True(value != value01);
        Assert.False(value == value01);
        Assert.NotEqual(value01, value);
    }

    #endregion

    #region Math constants

    [Fact]
    public void Check_Constant_E()
    {
        var diff = (BigDec.E - Math.E).Abs();
        Assert.True(diff <= 0.000_000_001m);
    }

    [Fact]
    public void Check_Constant_PI()
    {
        var diff = (BigDec.PI - Math.PI).Abs();
        Assert.True(diff <= 0.000_000_001m);
    }

    #endregion

    #region Fractional power of ten

    [Fact]
    public void FracPowerOfTenTest()
    {
        var m_0_1 = new BigDec(0.1m);

        foreach (var power in new int[] { 0, 1, 2, 10, 1000, 10_000 })
        {
            var expected = BigDec.One.WithPrecision(power + 1) / BigInteger.Pow(BigDec.BigInteger10, power);
            var actual1 = m_0_1.WithPrecision(power + 1).Pow(power);
            Assert.Equal(expected, actual1);

            var actual2 = BigDec.PowFractionOfTen(power);
            Assert.Equal(expected, actual2);
        }
    }

    #endregion
}
