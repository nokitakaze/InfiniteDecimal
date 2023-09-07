using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace InfiniteDecimal;

public partial class BigDec
{
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

    public bool IsZero()
    {
        return this.Value == BigInteger.Zero;
    }

    #region

    public BigDec Round(int decimalNumber)
    {
        // var pow = BigInteger.Pow(BigInteger10, maxPrecision);
        if (Offset <= decimalNumber)
        {
            return this.WithPrecision(decimalNumber);
        }

        var leftPow = BigInteger.Pow(BigInteger10, Offset - decimalNumber);
        var tail = this.Value % leftPow;

        var leftPow1 = leftPow;
        var tail1 = tail;
        while (tail1 > (BigInteger.One << 10))
        {
            tail1 >>= 8;
            leftPow1 >>= 8;
        }

        var u = !tail1.IsZero && ((leftPow1 / tail1) >= 2);

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
            x = 1.0 / x; // and take the reciprocal
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
        if (IsZero())
        {
            // ReSharper disable once ConvertIfStatementToReturnStatement
            if (exp.IsZero())
            {
                // 0 ^ 0 = 1
                // https://www.youtube.com/watch?v=OJ55XetZKF0
                return One;
            }

            return this;
        }

        if (this == One)
        {
            return this;
        }

        bool needReverse = false;
        if (exp < 0)
        {
            exp = -exp;
            needReverse = true;
        }

        var entier = exp.Floor();
        var tail = exp - entier;

        var result = Pow(entier);

        if (tail != Zero)
        {
            var expBase = tail * this.Ln();
            var tailPart = expBase.Exp();
            result *= tailPart;
        }

        if (needReverse)
        {
            result = One / result;
        }

        return result;
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
            return Zero;
        }

        if (this == One)
        {
            return One;
        }

        var half = new BigDec(0.5m);
        var epsilon = One / BigInteger.Pow(BigInteger10, this.MaxPrecision);
        var current = this.WithPrecision(this.MaxPrecision * 2) * half;
        while (true)
        {
            for (var i = 0; i < 30; i++)
            {
                current = (current + this / current) * half;
            }

            var a = current * current;
            var diff = (this - a).Abs();
            if (diff <= epsilon)
            {
                return current.Round(this.MaxPrecision);
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
            return Zero;
        }

        bool invert = (this <= 0.01m);
        var z = invert ? One / this : this.WithPrecision(this.MaxPrecision * 10);

        var result = BigDec.Zero;
        while (z >= 2)
        {
            result += One;
            z /= E;
        }

        if ((2 - z).Abs() < 0.001m)
        {
            result += One;
            z /= E;
        }

        var powDic = new Dictionary<int, BigInteger>();
        // Supported accuracy limit
        BigDec epsilon;
        {
            // ReSharper disable once RedundantCast
            int t = (int)Math.Max(MaxPrecision * 2 - (int)result, 0);
            var t1 = BigInteger.Pow(BigInteger10, t);
            powDic[t] = t1;
            epsilon = One.WithPrecision(t + 1) / t1;
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
        for (var i = 2; (!lastCycle || (i < 30)) && (i < 10_000) && !numenator.IsZero(); i++)
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

            if (numenator.Offset > numenator.MaxPrecision + 10)
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

    public BigDec Exp()
    {
        // Initial value for the result
        BigDec result = One;
        // Initial term of the series (for i=0)
        BigDec term = One;

        // Supported accuracy limit
        var epsilon = One.WithPrecision(MaxPrecision * 2 + 1) / BigInteger.Pow(BigInteger10, MaxPrecision * 2);
        // codecov ignore start
        if (epsilon <= Zero)
        {
            throw new InfiniteDecimalException($"Can't calculate r-component for precision '{MaxPrecision}'");
        }
        // codecov ignore end

        var tmpL = this.WithPrecision(MaxPrecision * 3);
        for (int i = 1; term.Abs() > epsilon; i++)
        {
            term *= tmpL / i;
            result += term;
        }

        return result;
    }

    #endregion
}