using System;
using System.Globalization;
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
        var parts = decimal.GetBits(value);
        var rawValue = new BigInteger((uint)parts[0]);
        rawValue |= new BigInteger((uint)parts[1]) << 32;
        rawValue |= new BigInteger((uint)parts[2]) << 64;

        // ReSharper disable once RedundantCast
        bool isNegative = ((uint)parts[3] & (uint)0x8000_0000) != 0;
        byte scale = (byte) ((parts[3] >> 16) & 0x7F);

        Offset = scale;
        Value = rawValue;
        if (isNegative)
        {
            Value = -Value;
        }

        this.NormalizeOffset();
    }

    public BigDec(decimal value, int maxPrecision) : this(value)
    {
        MaxPrecision = maxPrecision;
    }

    public BigDec(double value)
    {
        if (!double.IsFinite(value))
        {
            throw new InfiniteDecimalException($"value '{value}' is not finite");
        }

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

        var valueStringify = value.ToString("G17", CultureInfo.InvariantCulture);
        if (!valueStringify.Contains("E"))
        {
            var a = valueStringify.Split('.');
            if ((a.Length == 2) && (a[1].Length > 17))
            {
                valueStringify = a[0] + "." + a[1][..17].TrimEnd('0');
            }
        }

        var valueStringifyLength = valueStringify.Length;

        int addExp = 0;
        // Exponent notation
        {
            var rParse1 = new Regex(@"^9\.9+[0-9]{1,2}E\-(\d+)$");
            var m = rParse1.Match(valueStringify);
            if (m.Success)
            {
                var exp = int.Parse(m.Groups[1].Value);
                Value = sign;
                Offset = exp - 1;

                return;
            }

            var rParse2 = new Regex(@"^1E\-(\d+)$");
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
                var rParse3 = new Regex(@"^(.+?)E\-(\d+)$");
                m = rParse3.Match(valueStringify);
                // codecov ignore start
                if (!m.Success)
                {
                    throw new NotImplementedException($"Not implemented style '{value}'");
                }
                // codecov ignore end

                addExp = int.Parse(m.Groups[2].Value);
                valueStringify = m.Groups[1].Value;
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

    public BigDec(float value)
    {
        if (!float.IsFinite(value))
        {
            throw new InfiniteDecimalException($"value '{value}' is not finite");
        }

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

        var valueStringify = value.ToString("G9", CultureInfo.InvariantCulture);
        if (!valueStringify.Contains("E"))
        {
            var a = valueStringify.Split('.');
            if ((a.Length == 2) && (a[1].Length > 9))
            {
                valueStringify = a[0] + "." + a[1][..9].TrimEnd('0');
            }
        }

        var valueStringifyLength = valueStringify.Length;

        int addExp = 0;
        // Exponent notation
        {
            var rParse1 = new Regex(@"^9\.9+[0-9]{1,2}E\-(\d+)$");
            var m = rParse1.Match(valueStringify);
            if (m.Success)
            {
                var exp = int.Parse(m.Groups[1].Value);
                Value = sign;
                Offset = exp - 1;

                return;
            }

            var rParse2 = new Regex(@"^1E\-(\d+)$");
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
                var rParse3 = new Regex(@"^(.+?)E\-(\d+)$");
                m = rParse3.Match(valueStringify);
                // codecov ignore start
                if (!m.Success)
                {
                    throw new NotImplementedException($"Not implemented style '{value}'");
                }
                // codecov ignore end

                addExp = int.Parse(m.Groups[2].Value);
                valueStringify = m.Groups[1].Value;
                valueStringifyLength = valueStringify.Length;
            }
        }

        var bio = Parse(valueStringify);
        bio.NormalizeOffset();
        if (valueStringifyLength + addExp >= 9)
        {
            // TODO It is more correct to do via IEEE-754 mantissa size

            {
                var mod1_000_000 = bio.Value % 1_000_000;
                const int maxDiff = 55;
                if (mod1_000_000 == 0)
                {
                }
                else if (mod1_000_000 <= maxDiff)
                {
                    bio.Value -= mod1_000_000;
                }
                else if (mod1_000_000 >= 1_000_000 - maxDiff)
                {
                    bio.Value += 1_000_000 - mod1_000_000;
                }
            }

            /*
            {
                var mod100_000 = bio.Value % 100_000;
                const int maxDiff = 53;
                if (mod100_000 == 0)
                {
                }
                else if (mod100_000 <= maxDiff)
                {
                    bio.Value -= mod100_000;
                }
                else if (mod100_000 >= 100_000 - maxDiff)
                {
                    bio.Value += 100_000 - mod100_000;
                }
            }
            */

            {
                var mod10000 = bio.Value % 10_000;
                const int maxDiff = 53;
                if (mod10000 == 0)
                {
                }
                else if (mod10000 <= maxDiff)
                {
                    bio.Value -= mod10000;
                }
                else if (mod10000 >= 10_000 - maxDiff)
                {
                    bio.Value += 10_000 - mod10000;
                }
            }

            {
                var mod1000 = bio.Value % 1000;
                if (mod1000 == 0)
                {
                }
                else if (mod1000 <= 10)
                {
                    bio.Value -= mod1000;
                }
                else if (mod1000 >= 1000 - 10)
                {
                    bio.Value += 1000 - mod1000;
                }
            }
        }

        Value = bio.Value * sign;
        Offset = bio._offset + addExp;
        NormalizeOffset();
    }

    /// <summary>
    /// Returns a new instance of the BigDec class with the specified precision.
    /// </summary>
    /// <param name="newPrecision">The precision of the new BigDec instance.</param>
    /// <returns>A new instance of the BigDec class with the specified precision.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public BigDec WithPrecision(int newPrecision)
    {
        return new BigDec(this, newPrecision);
    }

    /// <summary>
    /// Creates a copy of the current BigDec object
    /// </summary>
    /// <returns>A new BigDec object that is a copy of the current instance.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public BigDec Copy()
    {
        return new BigDec(this);
    }
}
