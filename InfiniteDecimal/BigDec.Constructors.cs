using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace InfiniteDecimal;

public partial class BigDec
{
    public BigDec(BigDec value)
    {
        Value = value.Value;
        Offset = value.Offset;
        MaxPrecision = value.MaxPrecision;
    }

    public BigDec(BigDec value, int maxPrecision) : this(value)
    {
        MaxPrecision = maxPrecision;
    }

    public BigDec(BigInteger value)
    {
        Value = value;
    }

    public BigDec(long value) : this(new BigInteger(value))
    {
    }

    public BigDec(ulong value) : this(new BigInteger(value))
    {
    }

    public BigDec(decimal value)
    {
        var sign = 1;
        if (value < 0)
        {
            value = -value;
            sign = -1;
        }

        var decEntier = Math.Floor(value);
        var decTail = value - decEntier;

        if (decTail == 0)
        {
            var bi = new BigInteger(decEntier);
            Value = bi * sign;
            return;
        }

        var tailResult = BigInteger.Zero;
        const int OffsetStep = 9;
        var modifier = BigInteger.Pow(BigInteger10, OffsetStep);
        const decimal modifierDecimal = 1000_000_000m;
        while (decTail > 0)
        {
            Offset += OffsetStep;
            tailResult *= modifier;
            decTail *= modifierDecimal;

            var a = (long)Math.Floor(decTail);
            tailResult += a;
            decTail -= a;
        }

        Value = tailResult + (new BigInteger(decEntier)) * OffsetPower;
        Value *= sign;
        this.NormalizeOffset();
    }

    public BigDec(decimal value, int maxPrecision) : this(value)
    {
        MaxPrecision = maxPrecision;
    }

    public BigDec(double value)
    {
        if (value == 0)
        {
            Value = Zero.Value;
            Offset = Zero._offset;
            return;
        }

        var sign = 1;
        if (value < 0)
        {
            sign = -1;
            value = -value;
        }

        // ReSharper disable once CompareOfFloatsByEqualityOperator
        if (value == Math.Floor(value))
        {
            Value = new BigInteger(value) * sign;
            return;
        }

        var valueStringify = value.ToString("G17");
        var valueStringifyLength = valueStringify.Length;

        int addExp = 0;
        {
            var rParse1 = new Regex("^9\\.9+[0-9]{1,2}E\\-(\\d+)$");
            var m = rParse1.Match(valueStringify);
            if (m.Success)
            {
                var exp = int.Parse(m.Groups[1].Value);
                Value = sign;
                Offset = exp - 1;

                return;
            }

            var rParse2 = new Regex("^1E\\-(\\d+)$");
            m = rParse2.Match(valueStringify);
            if (m.Success)
            {
                var exp = int.Parse(m.Groups[1].Value);
                Value = sign;
                Offset = exp;

                return;
            }

            if (valueStringify.Contains("E"))
            {
                var rParse3 = new Regex("E\\-(\\d+)$");
                m = rParse3.Match(valueStringify);
                // codecov ignore start
                if (!m.Success)
                {
                    throw new NotImplementedException($"Not implemented style '{value}'");
                }
                // codecov ignore end

                addExp = int.Parse(m.Groups[1].Value);
                value *= Math.Pow(10d, addExp);
                valueStringify = value.ToString("G17");
                valueStringifyLength = valueStringify.Length;
            }
        }

        var bio = Parse(valueStringify);
        if (valueStringifyLength + addExp >= 18)
        {
            // TODO It is more correct to do via IEEE-754 mantissa size

            {
                var mod1_000_000 = bio.Value % 1_000_000;
                if (mod1_000_000 == 0)
                {
                }
                else if (mod1_000_000 <= 15)
                {
                    bio.Value -= mod1_000_000;
                }
                else if (mod1_000_000 >= 1_000_000 - 15)
                {
                    bio.Value += 1_000_000 - mod1_000_000;
                }
            }

            {
                var mod10000 = bio.Value % 10_000;
                if (mod10000 == 0)
                {
                }
                else if (mod10000 <= 10)
                {
                    bio.Value -= mod10000;
                }
                else if (mod10000 >= 10_000 - 10)
                {
                    bio.Value += 10_000 - mod10000;
                }
            }


            {
                var mod1000 = bio.Value % 1000;
                if (mod1000 == 0)
                {
                }
                else if (mod1000 <= 3)
                {
                    bio.Value -= mod1000;
                }
                else if (mod1000 >= 1000 - 3)
                {
                    bio.Value += 1000 - mod1000;
                }
            }
        }

        Value = bio.Value * sign;
        Offset = bio._offset + addExp;
        NormalizeOffset();
    }

    // TODO public BigDec(flaot value)

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public BigDec WithPrecision(int newPrecision)
    {
        return new BigDec(this, newPrecision);
    }
}