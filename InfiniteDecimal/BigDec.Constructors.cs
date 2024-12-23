using System;
using System.Globalization;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace InfiniteDecimal;

public partial class BigDec
{
    /// <summary>
    /// Copy constructor
    /// </summary>
    /// <param name="origin"></param>
    /// <remarks>This constructor has no meaning outside of this class since the class is immutable, so it is protected</remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected BigDec(BigDec origin) : this(origin._mantissa, origin._offset, origin.OffsetPower, origin.MaxPrecision)
    {
    }

    /// <summary>
    /// Copy constructor with max precision adjustment.
    /// Automatically truncates the mantissa if the offset exceeds max precision
    /// </summary>
    /// <param name="origin"></param>
    /// <param name="maxPrecision">The precision of the new BigDec instance.</param>
    public BigDec(BigDec origin, int maxPrecision) : this(
        origin._mantissa,
        origin._offset,
        origin.OffsetPower,
        maxPrecision
    )
    {
    }

    /// <summary>
    /// Constructor for creating an instance from all fields. Since the ability to set an incorrect offsetPower
    /// leads to dire consequences, this constructor is made protected.
    /// Use its public overload without offsetPower.
    /// Automatically truncates the mantissa if the offset exceeds max precision
    /// </summary>
    /// <param name="mantissa"></param>
    /// <param name="offset"></param>
    /// <param name="offsetPower"></param>
    /// <param name="maxPrecision">The precision of the new BigDec instance.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected BigDec(BigInteger mantissa, int offset, BigInteger offsetPower, int maxPrecision)
    {
        _mantissa = mantissa;
        _offset = offset;
        OffsetPower = offsetPower;
        MaxPrecision = maxPrecision;
        ReduceTrailingZeroes();
    }

    /// <summary>
    /// Constructor for creating an instance from all public fields. OffsetPower is set automatically.
    /// Automatically truncates the mantissa if the offset exceeds max precision
    /// </summary>
    /// <param name="mantissa"></param>
    /// <param name="offset"></param>
    /// <param name="maxPrecision">The precision of the new BigDec instance.</param>
    public BigDec(BigInteger mantissa, int offset, int maxPrecision)
    {
        _mantissa = mantissa;
        Offset = offset;
        MaxPrecision = maxPrecision;
        ReduceTrailingZeroes();
    }

    /// <summary>
    /// Constructor for creating an instance from BigInteger
    /// </summary>
    /// <param name="value">The value that the instance will be equal to. No offset is provided here</param>
    /// <param name="maxPrecision">The precision of the new BigDec instance.</param>
    public BigDec(BigInteger value, int maxPrecision = MaxDefaultPrecision)
    {
        _mantissa = value;
        MaxPrecision = maxPrecision;
    }

    /// <summary>
    /// Constructor for creating an instance from int64
    /// </summary>
    /// <param name="value">The value that the instance will be equal to. No offset is provided here</param>
    /// <param name="maxPrecision">The precision of the new BigDec instance.</param>
    public BigDec(long value, int maxPrecision = MaxDefaultPrecision) : this(new BigInteger(value), maxPrecision)
    {
    }

    /// <summary>
    /// Constructor for creating an instance from uint64
    /// </summary>
    /// <param name="value">The value that the instance will be equal to. No offset is provided here</param>
    /// <param name="maxPrecision">The precision of the new BigDec instance.</param>
    public BigDec(ulong value, int maxPrecision = MaxDefaultPrecision) : this(new BigInteger(value), maxPrecision)
    {
    }

    /// <summary>
    /// Constructor for creating an instance from MS decimal
    /// </summary>
    /// <param name="value">The value that the instance will be equal to. No offset is provided here</param>
    /// <param name="maxPrecision">The precision of the new BigDec instance.</param>
    public BigDec(decimal value, int maxPrecision = MaxDefaultPrecision) : this(value)
    {
        MaxPrecision = maxPrecision;
        ReduceTrailingZeroes();
    }

    /// <summary>
    /// Constructor for creating an instance from uint64
    /// </summary>
    /// <param name="value">The value that the instance will be equal to. No offset is provided here</param>
    public BigDec(decimal value)
    {
        var parts = decimal.GetBits(value);
        var rawValue = new BigInteger((uint)parts[0]);
        rawValue |= new BigInteger((uint)parts[1]) << 32;
        rawValue |= new BigInteger((uint)parts[2]) << 64;

        // ReSharper disable once RedundantCast
        bool isNegative = ((uint)parts[3] & (uint)0x8000_0000) != 0;
        byte scale = (byte)((parts[3] >> 16) & 0x7F);

        Offset = scale;
        _mantissa = rawValue;
        MaxPrecision = Math.Max(MaxDefaultPrecision, _offset);
        if (isNegative)
        {
            _mantissa = -_mantissa;
        }
    }

    /// <summary>
    /// Constructor for creating an instance from IEEE-754 double
    /// </summary>
    /// <param name="value">The value that the instance will be equal to. No offset is provided here</param>
    /// <param name="maxPrecision">The precision of the new BigDec instance.</param>
    public BigDec(double value, int maxPrecision = MaxDefaultPrecision)
    {
        if (!double.IsFinite(value))
        {
            throw new InfiniteDecimalException($"value '{value}' is not finite");
        }

        if (value == 0)
        {
            _mantissa = Zero._mantissa;
            Offset = Zero._offset;
            return;
        }

        MaxPrecision = maxPrecision;
        var sign = 1;
        if (value < 0)
        {
            sign = -1;
            value = -value;
        }

        // ReSharper disable once CompareOfFloatsByEqualityOperator
        if (value == Math.Floor(value))
        {
            _mantissa = new BigInteger(value) * sign;
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
                _mantissa = sign;
                Offset = exp - 1;

                return;
            }

            var rParse2 = new Regex(@"^1E\-(\d+)$");
            m = rParse2.Match(valueStringify);
            if (m.Success)
            {
                var exp = int.Parse(m.Groups[1].Value);
                _mantissa = sign;
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
                var mod1_000_000 = bio._mantissa % 1_000_000;
                if (mod1_000_000 == 0)
                {
                }
                else if (mod1_000_000 <= 15)
                {
                    bio._mantissa -= mod1_000_000;
                }
                else if (mod1_000_000 >= 1_000_000 - 15)
                {
                    bio._mantissa += 1_000_000 - mod1_000_000;
                }
            }

            {
                var mod10000 = bio._mantissa % 10_000;
                if (mod10000 == 0)
                {
                }
                else if (mod10000 <= 10)
                {
                    bio._mantissa -= mod10000;
                }
                else if (mod10000 >= 10_000 - 10)
                {
                    bio._mantissa += 10_000 - mod10000;
                }
            }

            {
                var mod1000 = bio._mantissa % 1000;
                if (mod1000 == 0)
                {
                }
                else if (mod1000 <= 3)
                {
                    bio._mantissa -= mod1000;
                }
                else if (mod1000 >= 1000 - 3)
                {
                    bio._mantissa += 1000 - mod1000;
                }
            }
        }

        _mantissa = bio._mantissa * sign;
        Offset = bio._offset + addExp;
        ReduceTrailingZeroes();
    }

    /// <summary>
    /// Constructor for creating an instance from IEEE-754 single
    /// </summary>
    /// <param name="value">The value that the instance will be equal to. No offset is provided here</param>
    /// <param name="maxPrecision">The precision of the new BigDec instance.</param>
    public BigDec(float value, int maxPrecision = MaxDefaultPrecision)
    {
        if (!float.IsFinite(value))
        {
            throw new InfiniteDecimalException($"value '{value}' is not finite");
        }

        if (value == 0)
        {
            _mantissa = Zero._mantissa;
            Offset = Zero._offset;
            return;
        }

        MaxPrecision = maxPrecision;
        var sign = 1;
        if (value < 0)
        {
            sign = -1;
            value = -value;
        }

        // ReSharper disable once CompareOfFloatsByEqualityOperator
        if (value == Math.Floor(value))
        {
            _mantissa = new BigInteger(value) * sign;
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
                _mantissa = sign;
                Offset = exp - 1;

                return;
            }

            var rParse2 = new Regex(@"^1E\-(\d+)$");
            m = rParse2.Match(valueStringify);
            if (m.Success)
            {
                var exp = int.Parse(m.Groups[1].Value);
                _mantissa = sign;
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
        bio.ReduceOffsetWhile10();
        if (valueStringifyLength + addExp >= 9)
        {
            // TODO It is more correct to do via IEEE-754 mantissa size

            {
                var mod1_000_000 = bio._mantissa % 1_000_000;
                const int maxDiff = 55;
                if (mod1_000_000 == 0)
                {
                }
                else if (mod1_000_000 <= maxDiff)
                {
                    bio._mantissa -= mod1_000_000;
                }
                else if (mod1_000_000 >= 1_000_000 - maxDiff)
                {
                    bio._mantissa += 1_000_000 - mod1_000_000;
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
                var mod10000 = bio._mantissa % 10_000;
                const int maxDiff = 53;
                if (mod10000 == 0)
                {
                }
                else if (mod10000 <= maxDiff)
                {
                    bio._mantissa -= mod10000;
                }
                else if (mod10000 >= 10_000 - maxDiff)
                {
                    bio._mantissa += 10_000 - mod10000;
                }
            }

            {
                var mod1000 = bio._mantissa % 1000;
                if (mod1000 == 0)
                {
                }
                else if (mod1000 <= 10)
                {
                    bio._mantissa -= mod1000;
                }
                else if (mod1000 >= 1000 - 10)
                {
                    bio._mantissa += 1000 - mod1000;
                }
            }
        }

        _mantissa = bio._mantissa * sign;
        Offset = bio._offset + addExp;
        ReduceTrailingZeroes();
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
}
