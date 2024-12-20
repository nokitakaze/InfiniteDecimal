﻿using System;
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
            return this.WithPrecision(decimalNumber);
        }

        var leftPow = Pow10BigInt(Offset - decimalNumber);
        var tail = this.Value % leftPow;

        var leftPow1 = leftPow;
        var tail1 = tail;
        while (tail1 > (BigInteger.One << 10))
        {
            tail1 >>= 8;
            leftPow1 >>= 8;
        }

        var u = !tail1.IsZero && ((leftPow1 / tail1) <= 1);

        var result = new BigDec(this);
        result.Value -= tail;
        result.Value /= leftPow;
        result.Offset = decimalNumber;
        if (u)
        {
            result.Value++;
        }

        result.NormalizeOffset();
        return result.WithPrecision(decimalNumber);
    }

    #endregion

    #region Power

    public BigDec Pow(BigInteger exp)
    {
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
            if (tail == 0.5m)
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

    public BigDec Sqrt()
    {
        if (this.Value < BigInteger.Zero)
        {
            throw new InfiniteDecimalException($"'{this}' below zero");
        }

        if (this == Zero)
        {
            return Zero.WithPrecision(this.MaxPrecision);
        }

        if (this == One)
        {
            return One.WithPrecision(this.MaxPrecision);
        }

        var epsilon = BigInteger.Pow(BigInteger10, PrecisionBuffer);
        // codecov ignore start
        if (epsilon <= Zero)
        {
            throw new InfiniteDecimalException($"Can't calculate r-component for precision '{MaxPrecision}'");
        }
        // codecov ignore end

        var addPowPow = this.MaxPrecision + PrecisionBuffer;
        var addPowValue = BigInteger.Pow(BigInteger10, addPowPow);
        var needPowerLevel = addPowPow * 2 - _offset;
        var expected = needPowerLevel switch
        {
            > 0 => this.Value * BigInteger.Pow(BigInteger10, needPowerLevel),
            0 => this.Value,
            _ => this.Value / BigInteger.Pow(BigInteger10, -needPowerLevel)
        };

        var expectedDiv = expected / addPowValue;
        var current = expectedDiv / 2;
        while (true)
        {
            for (var i = 0; i < 5; i++)
            {
                var currentT = (current + expected / current) / 2;
                if (currentT == current)
                {
                    // At this moment, we are in a situation whereit is impossible to approximate
                    // the value any further in any case
                    return new BigDec(current).WithPrecision(MaxPrecision) / addPowValue;
                }

                current = currentT;
            }

            var actual = current * current;
            var diff = BigInteger.Abs(expected - actual);
            // ReSharper disable once InvertIf
            if (diff <= epsilon)
            {
                return new BigDec(current).WithPrecision(MaxPrecision) / addPowValue;
            }
        }
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

        bool invert = (this <= 0.01m);
        var z = invert ? One / this : this.WithPrecision(this.MaxPrecision * 10);

        var result = BigDec.Zero;
        if (z > E)
        {
            var p1 = (long)Math.Floor(BigInteger.Log(z.Value) - z.Offset * Math.Log(10));
            var denumenator = E.Pow(p1);
            z /= denumenator;
            result += p1;

            while (z > E)
            {
                result += One;
                z /= E;
            }
        }

        // maximize the approximation of the value z to 1
        while ((1m - z).Abs() > 0.05)
        {
            var (exp, multiplier) = FoundExpPrecision(z);
            result -= exp;
            z *= multiplier;
        }

        z = z.Round(this.MaxPrecision + PrecisionBuffer);

        var powDic = new Dictionary<int, BigInteger>();
        // Supported accuracy limit
        BigDec epsilon;
        {
            // ReSharper disable once RedundantCast
            int t = (int)Math.Max(MaxPrecision * 2 - (int)result, PrecisionBuffer);
            var t1 = Pow10BigInt(t);
            powDic[t] = t1;
            epsilon = PowFracOfTen(t + 1);
        }

        // codecov ignore start
        if (epsilon <= Zero)
        {
            throw new InfiniteDecimalException($"Can't calculate r-component for precision '{MaxPrecision}'");
        }
        // codecov ignore end

        z -= One;
        var numenator = z;
        result += numenator;

        bool lastCycle = false;
        for (var i = 2; (!lastCycle || (i < 10)) && (i < 10_000) && !numenator.IsZero; i++)
        {
            numenator *= z;
            var tmp = numenator / i;
            lastCycle = (tmp.Abs() < epsilon);

            if ((i & 1) == 0)
            {
                result -= tmp;
            }
            else
            {
                result += tmp;
            }

            if (numenator.Offset > numenator.MaxPrecision + (PrecisionBuffer + 10))
            {
                var diff = numenator.Offset - numenator.MaxPrecision;
                BigInteger localDenumenator;
                if (BigInt10Powers.TryGetValue(diff, out var t1))
                {
                    localDenumenator = t1;
                }
                else if (powDic.TryGetValue(diff, out var t2))
                {
                    localDenumenator = t2;
                }
                else
                {
                    localDenumenator = BigInteger.Pow(BigInteger10, diff);
                    powDic[diff] = localDenumenator;
                }

                numenator.Value /= localDenumenator;
                numenator.Offset -= diff;
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
        var epsilon = PowFracOfTen(MaxPrecision + 3);
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
