using System;
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
        if (this._mantissa >= 0)
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
        return _mantissa / OffsetPower;
    }

    public bool IsZero => this._mantissa.IsZero;

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
        var tail = this._mantissa % leftPow;
        var tailDownAgain = new BigDec(tail, leftExpModifier) / leftPow;
        var value = _mantissa / leftPow;
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
        var biValue = this._mantissa;
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
            x = x.Inverse(); // and take the reciprocal
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
            return this.Inverse();
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
            var v = BigInteger.Log10(BigInteger.Abs(this._mantissa));
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
                t = t.WithPrecision(desiredPrecisionWithBuf).Inverse();
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
            result = this.WithPrecision(MaxPrecision * 2).Pow(entier);
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
            result = result.Inverse();
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
        if (this._mantissa < BigInteger.Zero)
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
            // At the point MaxPrecision is bigger or equal to Offset, it has been normalized in "this == One"
            var needPowerLevel = b * 2 - _offset;
            a = this._mantissa * Pow10BigInt(needPowerLevel);
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
        result._mantissa = aSqrt;
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
        var z = invert ? this.WithPrecision(zPrecision).Inverse() : this.WithPrecision(zPrecision);

        var result = BigDec.Zero;
        if (z > E)
        {
            var p1 = (long)Math.Floor(BigInteger.Log(z._mantissa) - z.Offset * Math.Log(10));
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

        z = z.Round(zPrecision) - One;
        BigInteger numerator;
        int numeratorPrecision;
        BigInteger resultWithinNumerator = BigInteger.Zero;
        {
            numeratorPrecision = z.MaxPrecision + PrecisionLnBuffer;
            var numeratorBD = z.WithPrecision(numeratorPrecision);
            result += numeratorBD;
            numerator = (BigInteger)(numeratorBD * BigDec.Pow10BigInt(numeratorPrecision));
        }

        bool lastCycle = false;
        for (var i = 2; (!lastCycle || (i < 10)) && (i < 10_000) && !numerator.IsZero; i++)
        {
            numerator = (BigInteger)(numerator * z);
            var tmp = numerator / i;
            lastCycle = (BigInteger.Abs(tmp) < BigInteger.One);

            if ((i & 1) == 0)
            {
                resultWithinNumerator -= tmp;
            }
            else
            {
                resultWithinNumerator += tmp;
            }
        }

        result += resultWithinNumerator * BigDec.PowFractionOfTen(numeratorPrecision);

        if (invert)
        {
            result = -result;
        }

        return result.Round(this.MaxPrecision);
    }

    #endregion

    #region Exp

    /// <summary>
    /// Calculates the exponential function of the current instance with Taylor-Maclaurin series
    /// </summary>
    /// <returns>
    /// The result of raising the mathematical constant e to the power of the current <see cref="BigDec"/> value.
    /// </returns>
    public BigDec Exp()
    {
        if (this < Zero)
        {
            return (-this).Exp().Inverse();
        }

        // Set accuracy limit to 0.001 of the precision
        int termPrecision = MaxPrecision + 4;

        BigDec simplifiedX;
        BigDec endedMultiplier;
        if (this >= One)
        {
            var t = this.Floor();
            endedMultiplier = BigDec.E.Pow(t);
            simplifiedX = (this - t).Round(termPrecision);
        }
        else
        {
            simplifiedX = this.Round(termPrecision);
            endedMultiplier = One;
        }

        {
            var index = Array.BinarySearch(ExpModifiers_exp, (decimal)simplifiedX);
            if (index <= 0)
            {
                index = -index - 1;
            }

            var (exp, multiplier) = ExpModifiers[index];
            endedMultiplier *= multiplier;
            simplifiedX -= exp;
        }

        var termPower = BigDec.Pow10BigInt(termPrecision);
        // Initial value for the result
        BigInteger result = termPower;
        // Initial term of the series (for i=0)
        BigInteger term = termPower;
        BigInteger simplifiedX_BI = (BigInteger)(simplifiedX * termPower);

        for (int i = 1; BigInteger.Abs(term) >= 10; i++)
        {
            term *= simplifiedX_BI / i;
            term /= termPower;
            result += term;
        }

        return new BigDec(
            endedMultiplier.Mantissa * result,
            endedMultiplier.Offset + termPrecision,
            MaxPrecision
        );
    }

    #endregion

    #region Inverse

    /// <summary>
    /// 1 / x or x^-1
    /// </summary>
    /// <returns></returns>
    public BigDec Inverse()
    {
        // 1 / (a * 10^-b) = 10^m / (a * 10^(m-b)) = 10^m / a * 10^-(m-b)
        var m = MaxPrecision + _offset;
        var numerator = BigDec.Pow10BigInt(m);
        var value = numerator / this.Mantissa;

        var t = new BigDec(value, MaxPrecision, MaxPrecision);
        return t;
    }

    #endregion
}
