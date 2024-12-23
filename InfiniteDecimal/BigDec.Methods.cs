using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace InfiniteDecimal;

public partial class BigDec
{
    /// <summary>
    /// Precision buffer used for inner calculations
    /// </summary>
    public const int PrecisionBuffer = 5;

    /// <summary>
    /// Precision buffer used for inner calculations in natural logarithm
    /// </summary>
    public const int PrecisionLnBuffer = 5;

    public BigDec Abs()
    {
        if (this.Value >= 0)
        {
            return this;
        }
        else
        {
            return -this;
        }
    }

    public BigInteger Floor()
    {
        return Value / OffsetPower;
    }

    public bool IsZero => this.Value.IsZero;

    #region

    public BigDec Round(int decimalNumber)
    {
        // var pow = GetPow10BigInt(maxPrecision);
        if (Offset <= decimalNumber)
        {
            return this.WithPrecision(Math.Max(decimalNumber, MaxDefaultPrecision));
        }

        var leftExpModifier = Offset - decimalNumber;
        var leftPow = Pow10BigInt(leftExpModifier);
        var tail = this.Value % leftPow;
        var tailDownAgain = new BigDec(tail, leftExpModifier) / leftPow;
        var value = Value / leftPow;
        if (tailDownAgain < Half)
        {
        }
        else if (tailDownAgain == Half)
        {
            // Round to Even
            if (!value.IsEven)
            {
                value++;
            }
        }
        else
        {
            value++;
        }

        var result = new BigDec(value, decimalNumber, decimalNumber);
        result.ReduceTrailingZeroes();
        return result;
    }

    public BigDec Floor(int decimalNumber)
    {
        if (this._offset <= decimalNumber)
        {
            return this;
        }

        var expDiff = _offset - decimalNumber;
        var denominator = Pow10BigInt(expDiff);
        var biValue = this.Value;
        biValue /= denominator;
        var result = new BigDec(biValue, this.Offset - expDiff, Math.Max(decimalNumber, MaxDefaultPrecision));
        return result;
    }

    #endregion

    #region Power

    public BigDec Pow(BigInteger exp)
    {
        if (IsZero && exp.IsZero)
        {
            return One.WithPrecision(MaxPrecision);
        }

        if (exp.IsOne)
        {
            return this;
        }

        var y = exp;
        if (y == 0)
        {
            // any number raised to the power of 0 equals 1
            return One;
        }

        var x = this;
        if (y < 0)
        {
            y = -y; // make the exponent positive
            x = One / x; // and take the reciprocal
        }

        BigDec result = One;
        while (y > 0)
        {
            // check for odd exponent
            if ((y & 1) == 1)
            {
                result *= x;
            }

            x *= x; // increase the base
            y >>= 1; // divide the exponent by 2

            if (x.Offset > x.MaxPrecision * 10)
            {
                x = x.Round(x.MaxPrecision);
            }
        }

        return result.Round(x.MaxPrecision);
    }

    public BigDec Pow(BigDec exp)
    {
        if (IsZero)
        {
            // ReSharper disable once ConvertIfStatementToReturnStatement
            if (exp.IsZero)
            {
                // 0 ^ 0 = 1
                // https://www.youtube.com/watch?v=OJ55XetZKF0
                return One.WithPrecision(this.MaxPrecision);
            }

            if (exp < Zero)
            {
                throw new InfiniteDecimalException(
                    "Operation cannot be performed: Raising zero to a negative power is undefined as it results in division by zero");
            }

            return this;
        }

        if (this == One)
        {
            return this;
        }

        if (exp == -One)
        {
            return One / this;
        }

        bool needReverse = false;
        if (exp < 0)
        {
            exp = -exp;
            needReverse = true;
        }

        int powAdditionalPrecision = 4;
        if (this.Offset > 0)
        {
            var v = BigInteger.Log10(BigInteger.Abs(this.Value));
            var bufferPrecision = 3 * (int)Math.Ceiling(this.Offset - v);
            powAdditionalPrecision += Math.Max(bufferPrecision, 0);
        }

        var desiredPrecision = Math.Max(exp.MaxPrecision, this.MaxPrecision);
        var desiredPrecisionWithBuf = desiredPrecision + powAdditionalPrecision;
        var entier = exp.Floor();
        var tail = exp - entier;

        if (tail.IsZero)
        {
            var powPrecision = Math.Max(desiredPrecisionWithBuf, this.Offset * (int)(BigInteger)exp);
            var t = this.WithPrecision(powPrecision).Pow((BigInteger)exp);
            // codecov ignore start
            if (t.IsZero)
            {
                throw new InfiniteDecimalException("Pow: Can't calculate proper inner operand");
            }
            // codecov ignore end

            if (needReverse)
            {
                // TODO too big exponent
                // var localPrecision = desiredPrecision + PrecisionBuffer * (int)(BigInteger)exp;
                t = One / t.WithPrecision(desiredPrecisionWithBuf);
            }

            return t.Round(desiredPrecision);
        }

        if (this < Zero)
        {
            // Complex numbers aren't implemented yet
            throw new NotImplementedException("Raising negative numbers to a fractional power is not implemented");
        }

        BigDec result;
        // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
        if (!needReverse)
        {
            result = Pow(entier);
        }
        else
        {
            // todo fix magic number
            result = this.WithPrecision(10_000).Pow(entier);
        }

        // tail.IsZero is always false condition
        // if (!tail.IsZero)
        {
            BigDec tailPart;
            if (tail == Half)
            {
                tailPart = Sqrt().WithPrecision(desiredPrecisionWithBuf);
            }
            else if (tail == 0.5m / 2)
            {
                tailPart = Sqrt().Sqrt().WithPrecision(desiredPrecisionWithBuf);
            }
            else if (tail == 0.5m / 4)
            {
                tailPart = Sqrt().Sqrt().Sqrt().WithPrecision(desiredPrecisionWithBuf);
            }
            else if (tail == 0.5m / 8)
            {
                tailPart = Sqrt().Sqrt().Sqrt().Sqrt().WithPrecision(desiredPrecisionWithBuf);
            }
            else
            {
                // Calculation via Taylor series.
                // a^b = e^(b * ln(a))
                var expBase = tail * this.WithPrecision(desiredPrecisionWithBuf).Ln();
                tailPart = expBase.Exp();
            }

            result *= tailPart;
        }

        if (needReverse)
        {
            result = One / result;
        }

        return result.Round(desiredPrecision);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public BigDec Pow(decimal powered)
    {
        return Pow(new BigDec(powered));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public BigDec Pow(double powered)
    {
        return Pow(new BigDec(powered));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public BigDec Pow(long powered)
    {
        return Pow(new BigInteger(powered));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public BigDec Pow(int powered)
    {
        return Pow(new BigInteger(powered));
    }

    #endregion

    #region Sqrt

    public static BigInteger Sqrt(BigInteger value)
    {
        if (value.IsZero)
        {
            return value;
        }

        if (value < 0)
        {
            throw new InfiniteDecimalException($"'{value}' below zero");
        }

        // Оценка начального приближения
        int bitLength = (int)Math.Ceiling(BigInteger.Log10(value) * Math.Log(10, 2));
        BigInteger root = BigInteger.One << (bitLength / 2);

        while (true)
        {
            BigInteger next = (root + value / root) >> 1;
            if ((next == root) || (next == root - 1))
            {
                return next;
            }

            root = next;
        }
    }

    public BigDec Sqrt()
    {
        if (this.Value < BigInteger.Zero)
        {
            throw new InfiniteDecimalException($"'{this}' below zero");
        }

        if (this.IsZero)
        {
            return Zero.WithPrecision(this.MaxPrecision);
        }

        if (this == One)
        {
            return One.WithPrecision(this.MaxPrecision);
        }

        // this = a * 10^-b
        // sqrt(this) = sqrt(a) * 10^(-0.5*b)

        BigInteger a;
        int b = this.MaxPrecision + PrecisionBuffer;

        {
            var needPowerLevel = b * 2 - _offset;
            a = needPowerLevel switch
            {
                > 0 => this.Value * Pow10BigInt(needPowerLevel),
                0 => this.Value,
                _ => this.Value / Pow10BigInt(-needPowerLevel)
            };
        }

        var aSqrt = Sqrt(a);
        var result = Zero.WithPrecision(MaxPrecision);
        if (b > MaxPrecision)
        {
            var c = Pow10BigInt(b - MaxPrecision);
            aSqrt /= c;
            b = MaxPrecision;
        }

        result.Offset = b;
        result.Value = aSqrt;
        result.ReduceOffsetWhile10();

        return result;
    }

    #endregion

    #region Ln

    /// <summary>
    /// Natural logarithm
    /// </summary>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public BigDec Ln()
    {
        if (this <= Zero)
        {
            throw new InfiniteDecimalException($"Value '{this}' <= 0; can't calculate natural logarithm");
        }

        if (this == One)
        {
            return Zero.WithPrecision(MaxPrecision);
        }

        var zPrecision = Math.Max(MaxPrecision, Offset) + PrecisionLnBuffer;
        bool invert = (this <= 0.01m);
        var z = invert ? One / this.WithPrecision(zPrecision) : this.WithPrecision(zPrecision);

        var result = BigDec.Zero;
        if (z > E)
        {
            var p1 = (long)Math.Floor(BigInteger.Log(z.Value) - z.Offset * Math.Log(10));
            var denominator = E.Pow(p1);
            z /= denominator;
            result += p1;

            while (z > E)
            {
                result += One;
                z /= E;
            }
        }

        // maximize the approximation of the value z to 1
        // 0.00098 ~= 1 / 1024, where 1024 is max power of E-group constants
        while ((One - z).Abs() >= 0.00098m)
        {
            var (exp, multiplier) = FoundExpPrecision(z);
            result -= exp;
            z *= multiplier;
        }

        var powDic = new Dictionary<int, BigInteger>();
        // Supported accuracy limit
        BigDec epsilon;
        {
            // ReSharper disable once RedundantCast
            int epsilonPrecision = MaxPrecision + PrecisionLnBuffer;
            var t1 = Pow10BigInt(epsilonPrecision);
            powDic[epsilonPrecision] = t1;
            epsilon = PowFractionOfTen(epsilonPrecision + 1);
        }

        // codecov ignore start
        if (epsilon <= Zero)
        {
            throw new InfiniteDecimalException($"Can't calculate r-component for precision '{MaxPrecision}'");
        }
        // codecov ignore end

        z = z.Round(zPrecision) - One;
        var numerator = z.WithPrecision(z.MaxPrecision + PrecisionLnBuffer);
        result += numerator;

        bool lastCycle = false;
        for (var i = 2; (!lastCycle || (i < 10)) && (i < 10_000) && !numerator.IsZero; i++)
        {
            numerator *= z;
            var tmp = numerator / i;
            lastCycle = (tmp.Abs() < epsilon);

            if ((i & 1) == 0)
            {
                result -= tmp;
            }
            else
            {
                result += tmp;
            }

            // ReSharper disable once InvertIf
            if (!lastCycle && (numerator.Offset > numerator.MaxPrecision + (PrecisionLnBuffer + 10)))
            {
                var diff = numerator.Offset - numerator.MaxPrecision - PrecisionLnBuffer;
                BigInteger localDenominator;
                if (BigInt10Powers.TryGetValue(diff, out var t1))
                {
                    localDenominator = t1;
                }
                else if (powDic.TryGetValue(diff, out var t2))
                {
                    localDenominator = t2;
                }
                else
                {
                    localDenominator = Pow10BigInt(diff);
                    powDic[diff] = localDenominator;
                }

                numerator.Value /= localDenominator;
                numerator.Offset -= diff;
            }
        }

        if (invert)
        {
            result = -result;
        }

        return result.Round(this.MaxPrecision);
    }

    #endregion

    #region Exp

    /// <summary>
    /// Calculates the exponential function of the current instance with Taylor series
    /// </summary>
    /// <returns>
    /// The result of raising the mathematical constant e to the power of the current <see cref="BigDec"/> value.
    /// </returns>
    public BigDec Exp()
    {
        // Initial value for the result
        BigDec result = One;
        // Initial term of the series (for i=0)
        BigDec term = One;

        // Set accuracy limit to 0.001 of the precision
        var epsilon = PowFractionOfTen(MaxPrecision + 3);
        // codecov ignore start
        if (epsilon <= Zero)
        {
            throw new InfiniteDecimalException($"Can't calculate r-component for precision '{MaxPrecision}'");
        }
        // codecov ignore end

        var innerTmpLPrecision = MaxPrecision + PrecisionBuffer;

        BigDec tmpL;
        BigDec endedMultiplier;
        if (this >= 2)
        {
            var t = this.Floor() - 1;
            endedMultiplier = BigDec.E.Pow(t);
            tmpL = (this - t).Round(innerTmpLPrecision);
        }
        else if (this < Zero)
        {
            var t = (-this).Floor() + 2;
            endedMultiplier = BigDec.One / BigDec.E.WithPrecision(innerTmpLPrecision).Pow(t);
            tmpL = (this + t).Round(innerTmpLPrecision);
        }
        else
        {
            tmpL = this.Round(innerTmpLPrecision);
            endedMultiplier = One;
        }

        for (int i = 1; term.Abs() >= epsilon; i++)
        {
            term *= tmpL / i;
            result += term;
        }

        return (result * endedMultiplier).Round(MaxPrecision);
    }

    #endregion
}
