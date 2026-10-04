using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using Xunit.Abstractions;

namespace InfiniteDecimal.Test;

[SuppressMessage("ReSharper", "UnusedMember.Global")]
public class TemporaryTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public TemporaryTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    // [Fact]
    public void TestInverse()
    {
        var powers = new int[] { 1, 2, 3, 5, 10, 15, 20, 30, 33, 50, 66, 80, 100, 200, 300, 500, 1000, };

        for (var mantissa = 1; mantissa <= 9; mantissa++)
        {
            foreach (var power in powers)
            {
                for (int precision = 0;; precision++)
                {
                    var i = BigInteger.Pow(new BigInteger(10), power) * mantissa - BigInteger.One;
                    var value = new BigDec(i, maxPrecision: precision);
                    var actual = value.Inverse();

                    if (actual.Mantissa.IsZero)
                    {
                        continue;
                    }

                    var lg = (int)Math.Ceiling(BigInteger.Log10(i));
                    _testOutputHelper.WriteLine($"{mantissa,1}\t{power,4}\t{precision - lg,3}");
                    break;
                }
            }
        }
    }

    // [Fact]
    public void TestSqrt()
    {
        var powers = new int[] { 2, 3, 5, 10, 15, 20, 30, 33, 50, 66, 80, 100, 200, 300, 500, 1000, };

        foreach (var power in powers)
        {
            var left = 0;
            var right = power * 10;

            while (left < right - 1)
            {
                var precision = (int)Math.Round((left + right) * 0.5);
                if (precision == left)
                {
                    precision++;
                }
                else if (precision == right)
                {
                    precision--;
                }

                var i1 = BigDec.Pow10BigInt(power + 1) + 5;
                var input = i1 * i1 + 75;
                input /= 100;

                var value = new BigDec(input, maxPrecision: precision);
                var actual = value.Sqrt();
                var actualMantissa = actual.Round(0).Mantissa;
                var expectedMantissa = BigDec.Pow10BigInt(power) + 1;

                if (actualMantissa == expectedMantissa)
                {
                    right = precision;
                }
                else
                {
                    left = precision;
                }
            }

            {
                var i1 = BigInteger.Pow(new BigInteger(10), power + 1) + 5;
                var input = i1 * i1 + 75;
                input /= 100;
                var lg = BigInteger.Log10(input);
                _testOutputHelper.WriteLine($"{power * 2,4}\t{right,4}\t{lg,11:F5}\t{right - (int)Math.Ceiling(lg),4}");
            }
        }
    }

    #region Pow

    // [Fact]
    public void Pow()
    {
        var based = new decimal[]
        {
            1.000_01m, 1.000_1m, 1.001m, 1.01m, 1.1m,
            10.000_01m, 10.000_1m, 10.001m, 10.01m, 10.1m,
            100.000_01m, 100.000_1m, 100.001m, 100.01m, 100.1m,
            0.27m,
        };
        based = based.Concat([
                // Проверка известного условия: результат должен быть 0.
                1m, 2m, 10m, 100m,

                // Промежуточные дробные части между имеющимися точками.
                1.000_001m,
                1.000_02m, 1.000_05m,
                1.000_2m, 1.000_5m,
                1.002m, 1.005m,
                1.02m, 1.05m,

                // Сейчас диапазон дробных частей выше 0.1 отсутствует.
                1.2m, 1.25m, 1.5m, 1.75m, 1.9m, 1.99m,

                // Подход к одному и тому же целому с двух сторон.
                1.999_99m, 2.000_01m,
                9.999_99m,
                99.999_99m,

                // Перенос зависимости на другие целые части.
                2.01m, 3.01m, 5.01m,
                20.01m, 50.01m, 200.01m, 1000.01m,

                // Сопоставление одинаковых дробных частей при разных масштабах.
                10.000_001m, 10.005m, 10.05m, 10.5m, 10.9m, 10.99m,
                100.000_001m, 100.005m, 100.05m, 100.5m, 100.9m, 100.99m
            ])
            .Distinct()
            .OrderBy(x => x)
            // ReSharper disable once UseCollectionExpression
            .ToArray();

        var powers = new int[] { 10, 20, 50, 100, 200, 500, 1000, 2000, 3000, 5000, 10_000 };
        powers = powers.Concat([
                // Поведение на малых степенях.
                1, 2, 3, 4, 5,

                // Окрестности степеней двойки: проверка возможных переходов.
                15, 16, 17,
                31, 32, 33,
                63, 64, 65,
                127, 128, 129,
                255, 256, 257,
                511, 512, 513,
                1023, 1024, 1025,
                2047, 2048, 2049,
                4095, 4096, 4097,
                8191, 8192, 8193,

                // Окрестности уже измеренных крупных значений.
                2999, 3001,
                4999, 5001,
                9999, 10001
            ])
            .Distinct()
            .OrderBy(x => x)
            // ReSharper disable once UseCollectionExpression
            .ToArray();

        var digitCounts = Enumerable.Range(1, 20).ToArray();

        foreach (var baseValue in based)
        {
            foreach (var power in powers)
            {
                foreach (var digitCount in digitCounts)
                {
                    CalcPrecisionForPower(baseValue, power, digitCount);
                }
            }
        }
    }

    protected void CalcPrecisionForPower(decimal baseValue, int power, int digitCount)
    {
        var value1 = new BigDec(baseValue, maxPrecision: 1_000_000);
        var expected = value1.Pow(power).Round(digitCount);

        var left = -1;
        var right = power * 10;

        while (left < right - 1)
        {
            var precision = Math.Max((int)Math.Round((left + right) * 0.5), 0);
            if (precision == left)
            {
                precision++;
            }
            else if (precision == right)
            {
                precision--;
            }

            var value = new BigDec(baseValue, maxPrecision: precision);
            var actual = value.Pow(power).Round(digitCount);
            if (expected == actual)
            {
                right = precision;
                if (right == 0)
                {
                    break;
                }
            }
            else
            {
                left = precision;
            }
        }

        // BigDec.PrecisionBuffer
        _testOutputHelper.WriteLine($"{baseValue}\t{power}\t{digitCount}\t{right * 2 + BigDec.PrecisionBuffer}");
    }

    #endregion

    #region Exp

    // [Fact]
    public void Exp()
    {
        var based = new decimal[]
        {
            1.000_01m, 1.000_1m, 1.001m, 1.01m, 1.1m,
            10.000_01m, 10.000_1m, 10.001m, 10.01m, 10.1m,
            100.000_01m, 100.000_1m, 100.001m, 100.01m, 100.1m,
            0.27m,
        };
        based = based.Concat([
                // Проверка известного условия: результат должен быть 0.
                1m, 2m, 10m, 20, 100m,

                // Промежуточные дробные части между имеющимися точками.
                1.000_001m,
                1.000_02m, 1.000_05m,
                1.000_2m, 1.000_5m,
                1.002m, 1.005m,
                1.02m, 1.05m,

                // Сейчас диапазон дробных частей выше 0.1 отсутствует.
                1.2m, 1.25m, 1.5m, 1.75m, 1.9m, 1.99m,

                // Подход к одному и тому же целому с двух сторон.
                1.999_99m, 2.000_01m,
                9.999_99m,
                99.999_99m,

                // Перенос зависимости на другие целые части.
                2.01m, 3.01m, 5.01m,
                20.01m, 50.01m, 200.01m, 1000.01m,

                // Сопоставление одинаковых дробных частей при разных масштабах.
                10.000_001m, 10.005m, 10.05m, 10.5m, 10.9m, 10.99m,
                100.000_001m, 100.005m, 100.05m, 100.5m, 100.9m, 100.99m
            ])
            .Distinct()
            .OrderBy(x => x)
            // ReSharper disable once UseCollectionExpression
            .ToArray();

        var digitCounts = Enumerable.Range(0, 21)
            .Concat([50, 100])
            .ToArray();

        foreach (var baseValue in based)
        {
            var expectedExp = new BigDec(baseValue, maxPrecision: 10_000);
            expectedExp = expectedExp.ExpWithBigPrecision();
            foreach (var digitCount in digitCounts)
            {
                CalcPrecisionForExp(baseValue, digitCount, expectedExp);
            }
        }
    }

    // [Fact]
    public void Exp1()
    {
        for (var baseValue = -10; baseValue <= 43; baseValue++)
        {
            var expectedExp = new BigDec(baseValue, maxPrecision: 10_000);
            expectedExp = expectedExp.ExpWithBigPrecision();
            CalcPrecisionForExp(baseValue, digitCount: 1, expectedExp);
        }
    }

    // [Fact]
    public void Exp2()
    {
        var diffs = new decimal[] { 0.1m, 0.2m, 0.3m, 0.5m, 0.7m, 0.9m };
        var values = new decimal[] { 0m, 5m, 10m, 20m, 30m, 40m, };

        foreach (var value in values)
        {
            foreach (var diff in diffs)
            {
                var baseValue = value + diff;
                var expectedExp = new BigDec(baseValue, maxPrecision: 10_000);
                expectedExp = expectedExp.ExpWithBigPrecision();
                CalcPrecisionForExp(baseValue, digitCount: 1, expectedExp);
            }
        }
    }

    protected void CalcPrecisionForExp(decimal baseValue, int digitCount, BigDec expectedExp)
    {
        var expected = expectedExp.Round(digitCount);

        var left = -1;
        var right = Math.Max(digitCount * 20, 100);

        while (left < right - 1)
        {
            var precision = Math.Max((int)Math.Round((left + right) * 0.5), 0);
            if (precision == left)
            {
                precision++;
            }
            else if (precision == right)
            {
                precision--;
            }

            var value = new BigDec(baseValue, maxPrecision: precision);
            var actual = value.ExpWithBigPrecision().Round(digitCount);
            if (expected == actual)
            {
                right = precision;
                if (right == 0)
                {
                    break;
                }
            }
            else
            {
                left = precision;
            }
        }

        {
            var value = new BigDec(baseValue, maxPrecision: right);
            var innerTermCount =
                BigDec.EstimateInnerPrecisionForLongExp(value.Mantissa, value.Offset, value.MaxPrecision);
            _testOutputHelper.WriteLine($"{baseValue}\t{digitCount}\t{innerTermCount}");
        }
    }

    #endregion

    #region Ln

    // [Fact]
    public void Ln()
    {
        var based = new decimal[]
        {
            1.000_01m, 1.000_1m, 1.001m, 1.01m, 1.1m,
            10.000_01m, 10.000_1m, 10.001m, 10.01m, 10.1m,
            100.000_01m, 100.000_1m, 100.001m, 100.01m, 100.1m,
            0.27m,
        };
        based = based.Concat([
                // Проверка известного условия: результат должен быть 0.
                1m, 2m, 10m, 20, 100m,

                // Промежуточные дробные части между имеющимися точками.
                1.000_001m,
                1.000_02m, 1.000_05m,
                1.000_2m, 1.000_5m,
                1.002m, 1.005m,
                1.02m, 1.05m,

                // Сейчас диапазон дробных частей выше 0.1 отсутствует.
                1.2m, 1.25m, 1.5m, 1.75m, 1.9m, 1.99m,

                // Подход к одному и тому же целому с двух сторон.
                1.999_99m, 2.000_01m,
                9.999_99m,
                99.999_99m,

                // Перенос зависимости на другие целые части.
                2.01m, 3.01m, 5.01m,
                20.01m, 50.01m, 200.01m, 1000.01m,

                // Сопоставление одинаковых дробных частей при разных масштабах.
                10.000_001m, 10.005m, 10.05m, 10.5m, 10.9m, 10.99m,
                100.000_001m, 100.005m, 100.05m, 100.5m, 100.9m, 100.99m
            ])
            .Distinct()
            .OrderBy(x => x)
            .Where(x => x > 0)
            // ReSharper disable once UseCollectionExpression
            .ToArray();
        decimal[] a =
        [
            0.1m, 0.01m, 0.001m, 0.0001m, 0.000_01m, 0.000_001m, 0.000_000_1m, 0.000_000_01m, 0.000_000_001m,
            0.2m, 0.02m, 0.002m, 0.0002m,
            0.3m, 0.03m, 0.003m, 0.0003m,
            0.4m, 0.04m, 0.004m, 0.0004m,
            0.5m, 0.05m, 0.005m, 0.0005m,
        ];

        based = based
            .Concat(a)
            .Concat(a.Select(t => 1m - t))
            .Distinct()
            .OrderBy(x => x)
            // ReSharper disable once UseCollectionExpression
            .ToArray();
        based = a
            .SelectMany(diff => new decimal[]
            {
                (decimal)(BigDec.E - diff),
                (decimal)(BigDec.E + diff),
            })
            .Distinct()
            .OrderBy(x => x)
            // ReSharper disable once UseCollectionExpression
            .ToArray();

        var digitCounts = Enumerable.Range(0, 21)
            .Concat([50, 100])
            .ToArray();

        foreach (var baseValue in based)
        {
            var expectedLn = new BigDec(baseValue, maxPrecision: 100_000);
            expectedLn = expectedLn.Ln();
            foreach (var digitCount in digitCounts)
            {
                CalcPrecisionForLn(baseValue, digitCount, expectedLn);
            }
        }
    }

    protected void CalcPrecisionForLn(decimal baseValue, int digitCount, BigDec expectedLn)
    {
        var expected = expectedLn.Round(digitCount);

        var origRight = Math.Max(digitCount * 100, 100);
        var left = -1;
        var right = origRight;

        while (left < right - 1)
        {
            var precision = Math.Max((int)Math.Round((left + right) * 0.5), 0);
            if (precision == left)
            {
                precision++;
            }
            else if (precision == right)
            {
                precision--;
            }

            var value = new BigDec(baseValue, maxPrecision: precision);
            if (value.IsZero)
            {
                // Мы убрали так много цифр, что число, которое было по модулю ниже 1, в 0 превратилось
                left = precision;
                continue;
            }

            var actual = value.Ln().Round(digitCount);
            if (expected == actual)
            {
                right = precision;
                if (right == 0)
                {
                    break;
                }
            }
            else
            {
                left = precision;
                if (left == origRight)
                {
                    origRight *= 10;
                    right = origRight;
                }
            }
        }

        _testOutputHelper.WriteLine($"{baseValue}\t{digitCount}\t{right + BigDec.PrecisionLnBuffer}");
    }

    #endregion
}
