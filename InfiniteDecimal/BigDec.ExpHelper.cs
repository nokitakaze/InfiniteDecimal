using System;
using System.Collections.Generic;
using System.Linq;

namespace InfiniteDecimal;

public partial class BigDec
{
    public static readonly (decimal exp, BigDec multiplier)[] ExpModifiers;

    private static (decimal, BigDec)[] GenerateExpModifiers()
    {
        double D = Math.Sqrt(Math.E);
        double C = Math.Sqrt(D);
        double B = Math.Sqrt(C);
        double A = Math.Sqrt(B);
        double AA = Math.Sqrt(A);

        const double limit1 = 1d / Math.E - 0.001d;
        const double limit2 = Math.E + 0.001d;

        var metaCounts = new Dictionary<decimal, (int[] counts, int count)>();
        const int MaxCount = 5;
        for (var count1 = -MaxCount; count1 <= MaxCount; count1++)
        {
            for (var count2 = -MaxCount; count2 <= MaxCount; count2++)
            {
                for (var count3 = -MaxCount; count3 <= MaxCount; count3++)
                {
                    for (var count4 = -MaxCount; count4 <= MaxCount; count4++)
                    {
                        for (var count5 = -MaxCount; count5 <= MaxCount; count5++)
                        {
                            for (var count6 = -MaxCount; count6 <= MaxCount; count6++)
                            {
                                decimal exp = count1 + count2 * 0.5m + count3 * 0.25m + count4 * 0.125m +
                                              count5 * (0.125m / 2) + count6 * (0.125m / 4);
                                if ((Math.Abs(exp) <= 0.001m) /* || (Math.Abs(1m - exp) <= 0.001m) */)
                                {
                                    continue;
                                }

                                var fullCount = Math.Abs(count1) + Math.Abs(count2) + Math.Abs(count3) +
                                                Math.Abs(count4) + Math.Abs(count5) + Math.Abs(count6);
                                if (metaCounts.ContainsKey(exp))
                                {
                                    if (fullCount >= metaCounts[exp].count)
                                    {
                                        continue;
                                    }
                                }

                                var multiplier = (count1 != 0) ? Math.Pow(Math.E, count1) : 1d;
                                multiplier *= (count2 != 0) ? Math.Pow(D, count2) : 1d;
                                multiplier *= (count3 != 0) ? Math.Pow(C, count3) : 1d;
                                multiplier *= (count4 != 0) ? Math.Pow(B, count4) : 1d;
                                multiplier *= (count5 != 0) ? Math.Pow(A, count5) : 1d;
                                multiplier *= (count6 != 0) ? Math.Pow(AA, count6) : 1d;
                                if (multiplier is < limit1 or > limit2)
                                {
                                    continue;
                                }

                                metaCounts[exp] = (
                                    new int[] { count1, count2, count3, count4, count5, count6, },
                                    fullCount
                                );
                            }
                        }
                    }
                }
            }
        }

        var modifiers = new BigDec[]
        {
            BigDec.E,
            BigDec.E_Sqrt,
            BigDec.E_Root4,
            BigDec.E_Root8,
            BigDec.E_Root16,
            BigDec.E_Root32,
        };

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
        foreach (var (index, min, max) in maxCounts)
        {
            var modifier = modifiers[index];
            var modifierR = BigDec.One / modifier;
            var dic = new Dictionary<int, BigDec> { { 0, BigDec.One }, { 1, modifier }, { -1, modifierR } };

            var current = modifier;
            for (var i = 2; i <= max; i++)
            {
                current *= modifier;
                dic[i] = current;
            }

            current = modifierR;
            for (var i = 2; i <= -min; i++)
            {
                current *= modifierR;
                dic[-i] = current;
            }

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

        return expModifiers.ToArray();
    }

    private static (decimal exp, BigDec multiplier) FoundExpPrecision(BigDec input)
    {
        if (input < 1 / E)
        {
            return ExpModifiers.Last();
        }

        if (input > E)
        {
            return ExpModifiers[0];
        }

        (decimal exp, BigDec multiplier) picked = ExpModifiers[0];
        double abs = double.PositiveInfinity;

        var inputRounded = input.Round(8);
        foreach (var item in ExpModifiers)
        {
            var r = item.multiplier * inputRounded;
            var newAbs = Math.Abs(1d - (double)r);
            // ReSharper disable once InvertIf
            if (newAbs < abs)
            {
                abs = newAbs;
                picked = item;
            }
        }

        return picked;
    }
}