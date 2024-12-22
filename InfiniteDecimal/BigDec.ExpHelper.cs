using System;
using System.Collections.Generic;
using System.Linq;

namespace InfiniteDecimal;

public partial class BigDec
{
    protected static readonly (decimal exp, BigDec multiplier)[] ExpModifiers;
    protected static readonly double[] ExpModifiers_multipliers;

    private static (decimal, BigDec)[] GenerateExpModifiers()
    {
        var modifiers = new BigDec[]
        {
            BigDec.E,
            BigDec.E_Sqrt,
            BigDec.E_Root4,
            BigDec.E_Root8,
            BigDec.E_Root16,
            BigDec.E_Root32,
            //*
            BigDec.E_Root64,
            BigDec.E_Root128,
            BigDec.E_Root256,
            BigDec.E_Root512,
            BigDec.E_Root1024,
            // */
        };

        var modifiersSimplified = modifiers
            .Select(t => (double)t)
            .ToArray();

        decimal[] exp1Modifiers;
        {
            var exp1ModifiersA = new List<decimal>(capacity: 20) { 1m };
            for (var i = 1; i < modifiers.Length; i++)
            {
                exp1ModifiersA.Add(exp1ModifiersA[i - 1] / 2);
            }

            exp1Modifiers = exp1ModifiersA.ToArray();
        }

        const double limit1 = 1d / Math.E - 0.001d;
        const double limit2 = Math.E + 0.001d;

        var a0 = new int[modifiers.Length];
        a0[0] = 1;
        var a1 = new int[modifiers.Length];
        a1[0] = -1;

        var metaCounts = new Dictionary<decimal, (int[] counts, int count)>
        {
            // Just for speed up
            [1m] = (a0, 1),
            [-1m] = (a1, 1),
        };

        const int MaxCount = 1;
        var currentPointer = Enumerable
            .Repeat(-MaxCount, modifiers.Length)
            .ToArray();
        currentPointer[0] = -5;

        while (true)
        {
            for (var z0 = 0; z0 < 1; z0++)
            {
                decimal exp = 0m;
                for (var i = 0; i < modifiers.Length; i++)
                {
                    exp += currentPointer[i] * exp1Modifiers[i];
                }

                if (Math.Abs(exp) <= 0.001m)
                {
                    // The offset is too small
                    break;
                }

                var fullCount = currentPointer.Sum(Math.Abs);
                if (metaCounts.TryGetValue(exp, out var count))
                {
                    if (fullCount >= count.count)
                    {
                        break;
                    }
                }

                var multiplier = 1d;
                for (var i = 0; i < modifiers.Length; i++)
                {
                    multiplier *= Math.Pow(modifiersSimplified[i], currentPointer[i]);
                }

                if (multiplier is < limit1 or > limit2)
                {
                    // These multipliers are useless; we obtain a number >0 & <E, and we need to approximate
                    // it to one by multiplication as much as possible, but these numbers
                    // are too big and too small for this task
                    break;
                }

                metaCounts[exp] = (currentPointer.ToArray(), fullCount);
            }

            //
            currentPointer[^1]++;
            var thatsAll = false;
            for (var i = currentPointer.Length - 1; i >= 0; i--)
            {
                var m = (i == 0) ? 5 : MaxCount;
                if (currentPointer[i] <= m)
                {
                    break;
                }

                if (i == 0)
                {
                    thatsAll = true;
                    break;
                }

                currentPointer[i] = -MaxCount;
                currentPointer[i - 1]++;
            }

            if (thatsAll)
            {
                break;
            }
        }

        var maxCounts = metaCounts
            .Select(t => t.Value.counts)
            .SelectMany(items => items.Select((count, index) => (count, index)))
            .GroupBy(t => t.index)
            .Select(t =>
            {
                var min = t.Min(t1 => t1.count);
                var max = t.Max(t1 => t1.count);
                return (t.Key, min, max);
            })
            .OrderBy(t => t.Key)
            .ToArray();
        var modifierDic = new Dictionary<int, Dictionary<int, BigDec>>();
        foreach (var (index, _, _) in maxCounts)
        {
            var modifier = modifiers[index];
            var modifierR = BigDec.One / modifier;
            var dic = new Dictionary<int, BigDec> { { 0, BigDec.One }, { 1, modifier }, { -1, modifierR } };

            modifierDic[index] = dic;
        }

        var expModifiers = new List<(decimal, BigDec)>();
        foreach (var (exp, (counts, _)) in metaCounts)
        {
            var multiplier = BigDec.One;
            for (var i = 0; i < counts.Length; i++)
            {
                if (counts[i] == 0)
                {
                    continue;
                }

                multiplier *= modifierDic[i][counts[i]];
            }

            expModifiers.Add((exp, multiplier.Round(E.MaxPrecision)));
        }

        return expModifiers
            .OrderBy(t => t.Item1)
            .ToArray();
    }

    /// <summary>
    /// Selecting the most suitable exponent degree modifier
    /// </summary>
    /// <param name="input">Power, for which we are looking for the best exponent. Here we expect a number "greater than 0 and less than E."</param>
    /// <returns></returns>
    private static (decimal exp, BigDec multiplier) FoundExpPrecision(BigDec input)
    {
        if (input < E_inverse)
        {
            // Biggest value
            return ExpModifiers.Last();
        }

        if (input > E)
        {
            // Lowest value
            return ExpModifiers[0];
        }

        (decimal exp, BigDec multiplier) picked = ExpModifiers[0];
        double abs = double.PositiveInfinity;

        var inputRounded = (double)input.Round(8);
        var wantedMultiplier = 1d / inputRounded;
        var nearestIndex = Math.Abs(Array.BinarySearch(ExpModifiers_multipliers, wantedMultiplier));

        for (
            var index = Math.Max(0, nearestIndex - 3);
            index < Math.Min(ExpModifiers.Length, nearestIndex + 4);
            index++
        )
        {
            var item = ExpModifiers[index];
            var r = (double)item.multiplier * inputRounded;
            var newAbs = Math.Abs(1d - r);
            if (newAbs > abs)
            {
                return picked;
            }

            abs = newAbs;
            picked = item;
        }

        return picked;
    }
}
