using System;
using System.Globalization;
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
        if (this.Mantissa >= 0)
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
        if (Mantissa.IsZero)
        {
            return BigInteger.Zero;
        }

        if (Mantissa.Sign == 1)
        {
            // Positive number
            return Mantissa / OffsetPower;
        }

        // Negative number.
        // The floor always tends toward negative infinity.
        var result = Mantissa / OffsetPower;
        if (!(-Mantissa % OffsetPower).IsZero)
        {
            result--;
        }

        return result;
    }

    public bool IsZero => this.Mantissa.IsZero;

    #region

    public BigDec Round(int decimalNumber)
    {
        if (Mantissa.IsZero)
        {
            return new BigDec(BigInteger.Zero, 0, BigInteger.One, MaxPrecision);
        }

        if (Offset <= decimalNumber)
        {
            return this.WithPrecision(Math.Max(decimalNumber, MaxDefaultPrecision));
        }

        int sign;
        BigInteger mantissa;
        if (Mantissa.Sign >= 0)
        {
            sign = 1;
            mantissa = Mantissa;
        }
        else
        {
            sign = -1;
            mantissa = -Mantissa;
        }

        var leftExpModifier = Offset - decimalNumber;
        var leftPow = Pow10BigInt(leftExpModifier);
        var tail = mantissa % leftPow;
        var tailDownAgain = new BigDec(tail, leftExpModifier) / leftPow;
        var value = mantissa / leftPow;
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

        var offset = decimalNumber;
        var offsetPower = Pow10BigInt(offset);
        ReduceOverflowPrecision(ref value, ref offset, ref offsetPower, decimalNumber);
        if (sign == -1)
        {
            value = -value;
        }

        var result = new BigDec(value, offset, offsetPower, decimalNumber);

        return result;
    }

    public BigDec Floor(int decimalNumber)
    {
        if (this.IsZero)
        {
            return Zero;
        }

        if (this.Offset <= decimalNumber)
        {
            return this;
        }

        if (decimalNumber == 0)
        {
            return new BigDec(this.Floor(), 0, maxPrecision: this.MaxPrecision);
        }

        var expDiff = Offset - decimalNumber;
        var denominator = Pow10BigInt(expDiff);
        var biValue = this.Mantissa;
        biValue /= denominator;
        if ((this.Mantissa.Sign == -1) && !(-this.Mantissa % denominator).IsZero)
        {
            biValue--;
        }

        var newOffset = this.Offset - expDiff;
        var offsetPower = Pow10BigInt(newOffset);
        var maxPrecision = Math.Max(decimalNumber, MaxDefaultPrecision);
        ReduceOverflowPrecision(ref biValue, ref newOffset, ref offsetPower, maxPrecision, roundingMode: 1);
        var result = new BigDec(biValue, newOffset, maxPrecision: maxPrecision);
        return result;
    }

    #endregion

    #region Power

    public const int PrecisionPowBuffer = 5;

    public static int EstimateInnerPrecisionForPow(BigInteger X, int Y, BigInteger power, int needPrecision)
    {
        if (Y < 0)
            throw new ArgumentOutOfRangeException(nameof(Y), $"{Y} < 0");

        if (power == 0)
            return Math.Max(needPrecision, PrecisionPowBuffer);

        if (power < 0)
            power = -power;

        // При нормированном представлении основание целое ⇔ Y == 0.
        if (Y == 0)
            return PrecisionPowBuffer;

        // Как и прежде, для нецелых оснований поддерживается a >= 1.
        // Проверяем X >= 10^Y без построения огромной степени.
        X = BigInteger.Abs(X);

        double d = Y;
        var l = Math.Max(BigInteger.Log10(X), 1) - Y;
        var denominator = d + l;

        var powerD = (double)power;
        var cap = powerD * d;
        var stepped = d * Math.Ceiling((powerD * l + needPrecision + 3.0) / denominator);

        var correction = Math.Max(
            0.0,
            (8.0 + d - Math.Log10(powerD) - 3.0 * l) / 2.0
        );

        var smooth = powerD * l * d / denominator + needPrecision + correction;
        var bound = Math.Min(cap, Math.Min(stepped, smooth));

        return Math.Max(checked(PrecisionPowBuffer + 2 * (int)Math.Floor(bound)), needPrecision + PrecisionPowBuffer);
    }

    public BigDec Pow(BigInteger exp)
    {
        if (exp.IsZero)
        {
            // any number raised to the power of 0 equals 1
            return One.WithPrecision(MaxPrecision);
        }

        if (IsZero && (exp < BigInteger.Zero))
        {
            throw new DivideByZeroException($"Can't compute 0^{exp}");
        }

        if (exp.IsOne)
        {
            return this;
        }

        if (exp.Sign == -1)
        {
            if (-exp <= int.MaxValue)
            {
                // ReSharper disable once RedundantOverflowCheckingContext
                int localPrecision = checked((int)(-exp * this.MaxPrecision) + PrecisionBuffer);
                var r = this
                    .WithPrecision(localPrecision)
                    .Pow(-exp)
                    .Inverse();
                var (rMantissa, rOffset, rPower, _) = r;
                ReduceOverflowPrecision(ref rMantissa, ref rOffset, ref rPower, MaxPrecision);
                return new BigDec(rMantissa, rOffset, rPower, MaxPrecision);
            }
            else
            {
                return Pow(-exp).Inverse();
            }
        }

        // var currentPrecision = MaxPrecision + PrecisionBuffer;
        var currentPrecision = EstimateInnerPrecisionForPow(this.Mantissa, this.Offset, exp, MaxPrecision);

        var x = this.WithPrecision(currentPrecision);
        var y = exp;
        BigDec result = One;
        while (y > 0)
        {
            // check for odd exponent
            if (!y.IsEven)
            {
                result *= x;
            }

            x *= x; // increase the base
            y >>= 1; // divide the exponent by 2
            // Offset is always limited to MaxPrecision
        }

        return result.Round(this.MaxPrecision);
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
                throw new DivideByZeroException(
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

        if ((this == MinusOne) && (exp.Offset == 0))
        {
            return exp.Mantissa.IsEven ? One.WithPrecision(this.MaxPrecision) : this;
        }

        bool needReverse = false;
        if (exp < 0)
        {
            exp = -exp;
            needReverse = true;
        }

        var mantissaDigitCount = (int)Math.Max(Math.Ceiling(BigInteger.Log10(BigInteger.Abs(this.Mantissa))), 1);
        int powAdditionalPrecision = 4;
        if (this.Offset > 0)
        {
            var bufferPrecision = 3 * (this.Offset - mantissaDigitCount);
            powAdditionalPrecision += Math.Max(bufferPrecision, 0);
        }

        // todo через BigInteger.DivRem();
        var entier = exp.Floor();
        var tail = exp - entier;

        var desiredPrecision = Math.Max(exp.MaxPrecision, this.MaxPrecision);
        var desiredPrecisionWithBuf = desiredPrecision + powAdditionalPrecision;
        desiredPrecisionWithBuf = Math.Max(desiredPrecisionWithBuf, (int)Math.Ceiling(this.Offset * (double)exp));
        if (needReverse)
        {
            desiredPrecisionWithBuf *= 2;
            var a = this.Offset - mantissaDigitCount + 1;
            if (a > 0)
            {
                desiredPrecisionWithBuf += (int)Math.Ceiling((double)(a * exp));
            }
        }
        else
        {
            desiredPrecisionWithBuf += (int)Math.Ceiling((double)(mantissaDigitCount * exp));
        }

        var currentThis = this.WithPrecision(desiredPrecisionWithBuf);

        if (tail.IsZero)
        {
            var t = currentThis.Pow(entier);
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

        var result = currentThis.Pow(entier);

        var expDigitCount = (int)Math.Max(Math.Ceiling(BigInteger.Log10(BigInteger.Abs(tail.Mantissa))), 1);
        if ((tail <= 0.001m) && (expDigitCount <= 4))
        {
            var digitBuffer = Math.Max(1000, desiredPrecisionWithBuf);
            var temp = currentThis.Pow(tail.Mantissa);
            var m1 = temp.Mantissa;
            var o1 = temp.Offset;
            for (var i = 0; i < tail.Offset; i++)
            {
                if (BigInteger.Log10(m1) < digitBuffer)
                {
                    m1 *= Pow10BigInt(digitBuffer);
                    o1 += digitBuffer;
                }

                var o1rem = 10 - (o1 % 10);
                if (o1rem > 0)
                {
                    m1 *= Pow10BigInt(o1rem);
                    o1 += o1rem;
                }

                m1 = CalculateNthRoot(m1, 10);
                o1 /= 10;
            }

            temp = new BigDec(m1, offset: o1, maxPrecision: desiredPrecisionWithBuf);
            result *= temp;
        }
        else
        {
            BigDec tailPart;
            // TODO Вычислить через tail = 2^-N
            if (tail == Half)
            {
                tailPart = currentThis.Sqrt();
            }
            else if (tail == 0.5m / 2)
            {
                tailPart = currentThis.Sqrt().Sqrt();
            }
            else if (tail == 0.5m / 4)
            {
                tailPart = currentThis.Sqrt().Sqrt().Sqrt();
            }
            else if (tail == 0.5m / 8)
            {
                tailPart = currentThis.Sqrt().Sqrt().Sqrt().Sqrt();
            }
            else
            {
                // Calculation via Taylor series.
                // a^b = e^(b * ln(a))
                var expBase = tail * currentThis.Ln();
                tailPart = expBase.Exp();
            }

            result *= tailPart;
        }

        if (needReverse)
        {
            result = result.Inverse();
        }

        return result.Round(desiredPrecision).WithPrecision(desiredPrecision);
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

    /// <summary>
    /// Returns the nearest integer square root
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    /// <exception cref="InfiniteDecimalException"></exception>
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

        // Initial approximation evaluation
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
        if (this.Mantissa < BigInteger.Zero)
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
        int b = this.MaxPrecision + PrecisionBuffer + (int)Math.Ceiling(BigInteger.Log10(Mantissa) * 0.5d);

        {
            // At the point MaxPrecision is bigger or equal to Offset, it has been normalized in "this == One"
            var needPowerLevel = b * 2 - Offset;
            a = this.Mantissa * Pow10BigInt(needPowerLevel);
        }

        var aSqrt = Sqrt(a);
        if (BigInteger.Pow(aSqrt, 2) < a)
        {
            aSqrt++;
        }

        return new BigDec(aSqrt, b, MaxPrecision);
    }

    #endregion

    #region Nth root

    public static BigInteger CalculateNthRoot(BigInteger value, int degree)
    {
        if (value < 0)
            throw new InfiniteDecimalException($"'{value}' below zero");

        if (degree < 2)
            throw new InfiniteDecimalException($"'{degree}' below 2");

        if (value < 2)
            return value;

        // Находим верхнюю границу
        BigInteger low = 0;
        BigInteger high = 1;

        while (BigInteger.Pow(high, degree) <= value)
            high <<= 1; // high *= 2

        // Бинарный поиск
        while (high - low > 1)
        {
            BigInteger mid = (low + high) >> 1;

            if (BigInteger.Pow(mid, degree) <= value)
                low = mid;
            else
                high = mid;
        }

        return low;
    }

    #endregion

    #region Ln

    public static int EstimateInnerPrecisionForLn(
        BigInteger X,
        int Y,
        int needPrecision
    )
    {
        if (X.Sign < 0)
            throw new ArgumentOutOfRangeException(nameof(X));

        if (Y < 0)
            throw new ArgumentOutOfRangeException(nameof(Y));

        if (needPrecision < 0)
            throw new ArgumentOutOfRangeException(nameof(needPrecision));

        var p = (long)needPrecision;

        // Ноль обрабатывается по указанному Вами специальному правилу.
        // Для нормированного представления value == 1 означает X == 1, Y == 0.
        if (X.IsZero || (X.IsOne && Y == 0))
            return checked((int)(p + PrecisionLnBuffer));

        string digits = X.ToString(CultureInfo.InvariantCulture);

        // Точный floor(log10(value)), без вычисления логарифма.
        long order = (long)digits.Length - Y - 1;
        long extra = 3;

        // Здесь 0.1 <= value < 10: можно исследовать расстояние до 1.
        if (order is -1 or 0)
        {
            BigInteger unit = BigInteger.Pow(10, Y);
            BigInteger distance = BigInteger.Abs(X - unit);

            // |value - 1| < 0.4 * 10^(-p).
            // Сравнение полностью целочисленное.
            if (p < Y && 10 * distance < 4 * Pow10BigInt(Y - (int)p))
            {
                return PrecisionLnBuffer;
            }

            if (order == -1)
            {
                // k = Ceiling(-Log10(1 - value)).
                long k = (long)Y
                         - distance.ToString(CultureInfo.InvariantCulture).Length
                         + 1;

                // Покрывает наблюдаемые скачки слева от 1.
                if (p == 2 * k)
                    extra = Math.Max(extra, k + 1);

                if (p == k - 1)
                    extra = Math.Max(extra, k);
            }
        }

        // Участок около e, где таблица допускает результат 5.
        // При order == 0 имеем 1 <= value < 10.
        if (order == 0 && p <= 12)
        {
            int count = Math.Min(16, digits.Length);

            double leading = double.Parse(digits[..count], CultureInfo.InvariantCulture);
            double value = leading / Math.Pow(10.0, count - 1);
            double lnValue = Math.Log(value);

            // Запас учитывает неточность короткой мантиссы и double.
            // Для более высокой точности эта оптимизация не применяется.
            if (Math.Abs(lnValue - 1.0) + 1e-13 < 0.49 * Math.Pow(10.0, -p))
            {
                return PrecisionLnBuffer;
            }
        }

        long smallValueMinimum = Math.Max(0L, -order);
        long result = PrecisionLnBuffer + Math.Max(p + extra, smallValueMinimum);

        return checked((int)result);
    }

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

        var zPrecision = EstimateInnerPrecisionForLn(Mantissa, Offset, MaxPrecision);
        bool invert = (this <= 0.01m);
        var z = invert ? this.WithPrecision(zPrecision).Inverse() : this.WithPrecision(zPrecision);

        var result = BigDec.Zero;
        if (z > E)
        {
            var p1 = (long)Math.Floor(BigInteger.Log(z.Mantissa) - z.Offset * Math.Log(10));
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

    public static int EstimateInnerPrecisionForLongExp(
        BigInteger X,
        int Y,
        int needPrecision
    )
    {
        if (Y < 0)
            throw new ArgumentOutOfRangeException(nameof(Y));

        if (needPrecision < 0)
            throw new ArgumentOutOfRangeException(nameof(needPrecision));

        if (needPrecision == 0 && Y == 0)
        {
            if (X <= 7)
                return 4;

            if (X <= 13)
                return 12;

            // 2 * Ceiling((0.4343 * X + 6) / 2).
            // Точное целочисленное вычисление без double.
            BigInteger result = 2 * ((4343 * X + 79999) / 20000);

            return (int)result;
        }

        long p = needPrecision;

        // hint: 0.4343 ≈ ln(10)^-1
        // magnitude = Ceiling(0.4343 * Abs(value)).
        // Все вычисления точные, без преобразования BigInteger в double.
        BigInteger numerator = BigInteger.Abs(X) * 4343;
        long scale = (long)Y + 4;

        var numeratorDigitCount = Math.Max((int)Math.Ceiling(BigInteger.Log10(BigInteger.Abs(numerator))), 1);

        BigInteger magnitude;
        if (scale >= numeratorDigitCount)
        {
            // 0 < numerator / 10^scale < 1.
            // Заодно избегаем построения огромного 10^scale при большом Y.
            magnitude = BigInteger.One;
        }
        else
        {
            BigInteger denominator = BigInteger.Pow(10, (int)scale);

            magnitude = BigInteger.DivRem(numerator, denominator, out BigInteger remainder);

            if (!remainder.IsZero)
                magnitude++;
        }

        // Значения выше cap уже не влияют на итог.
        long magnitudePart = (long)magnitude;

        long estimate = Math.Max(
            2L * p + 10,
            Math.Max(p + magnitudePart + 5, 2L * Y + 4)
        );

        // Округление вверх до чётного числа и ограничение сверху.
        estimate = (estimate + 1) & ~1L;

        // Не допускаем переполнения с возвратом заниженного результата.
        return checked((int)estimate);
    }

    /// <summary>
    /// Calculates the exponential function of the current instance with Taylor-Maclaurin series
    /// </summary>
    /// <returns>
    /// The result of raising the mathematical constant e to the power of the current <see cref="BigDec"/> value.
    /// </returns>
    public BigDec Exp()
    {
        if (this.IsZero)
        {
            return One.WithPrecision(this.MaxPrecision);
        }

        // Set accuracy limit to 0.001 of the precision
        int termPrecision = EstimateInnerPrecisionForLongExp(this.Mantissa, this.Offset, this.MaxPrecision);

        if (this < Zero)
        {
            return (-this)
                .WithPrecision(termPrecision)
                .Exp()
                .Inverse()
                .WithPrecision(MaxPrecision);
        }

        BigDec simplifiedX;
        BigDec endedMultiplier;
        if (this >= One)
        {
            var t = this.Floor();
            endedMultiplier = BigDec.E.Pow(t);
            simplifiedX = (this - t).Round(termPrecision);
            if (simplifiedX.IsZero)
            {
                return endedMultiplier.WithPrecision(MaxPrecision);
            }
        }
        else
        {
            simplifiedX = this.Round(termPrecision);
            endedMultiplier = One;
        }

        if ((simplifiedX >= MinAbsDecimalValue) && (simplifiedX <= MaxDecimalValue))
        {
            var index = Array.BinarySearch(ExpModifiers_exp, (decimal)simplifiedX);
            if (index <= 0)
            {
                index = -index - 1;
            }

            var (exp, multiplier) = ExpModifiers[index];
            endedMultiplier *= multiplier;
            simplifiedX -= exp;
            if (simplifiedX.IsZero)
            {
                return endedMultiplier.WithPrecision(MaxPrecision);
            }
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

    /// <summary>
    /// Calculates the exponential function of the current instance with Taylor-Maclaurin series.
    /// This method does not use acceleration through the built-in Euler constant, which is limited to 1000 digits.
    /// If you do not need precision above 999 digits, use <see cref="Exp"/> instead.
    /// </summary>
    /// <returns>
    /// The result of raising the mathematical constant e to the power of the current <see cref="BigDec"/> value.
    /// </returns>
    public BigDec ExpWithBigPrecision()
    {
        // Set accuracy limit to 0.001 of the precision
        int termPrecision = EstimateInnerPrecisionForLongExp(this.Mantissa, this.Offset, this.MaxPrecision);

        if (this < Zero)
        {
            return (-this)
                .WithPrecision(termPrecision)
                .ExpWithBigPrecision()
                .Inverse()
                .WithPrecision(MaxPrecision);
        }

        var localMantissa = Mantissa * BigDec.Pow10BigInt(termPrecision - Offset);

        var termPower = BigDec.Pow10BigInt(termPrecision);
        // Initial value for the result
        BigInteger result = termPower;
        // Initial term of the series (for i=0)
        BigInteger term = termPower;

        for (int i = 1; BigInteger.Abs(term) >= 10; i++)
        {
            term *= localMantissa / i;
            term /= termPower;
            result += term;
        }

        return new BigDec(
            result,
            termPrecision,
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
        if (this.IsZero)
        {
            throw new DivideByZeroException("Can't compute 0^-1");
        }

        // 1 / (a * 10^-b) = 10^m / (a * 10^(m-b)) = 10^m / a * 10^-(m-b)
        var precisionBuffer = PrecisionBuffer;
        precisionBuffer += Math.Max(0, (int)Math.Ceiling(BigInteger.Log10(BigInteger.Abs(Mantissa))) - Offset);

        var m = MaxPrecision + Offset + precisionBuffer;
        var numerator = BigDec.Pow10BigInt(m);
        var denominator = BigDec.Pow10BigInt(precisionBuffer);

        var sign = this.Mantissa.Sign;
        var value = BigInteger.Abs(numerator / this.Mantissa);
        var remainder = value % denominator;
        var half = 5 * Pow10BigInt(precisionBuffer - 1);
        value /= denominator;
        if (remainder > half)
        {
            value++;
        }

        /*
         * "remainder" CAN'T BE exactly half for any uneven entier
         * ------------------------------
         * For any finite decimal value whose fractional part is 0.5 and whose integer
         * part is odd, let a = (2k + 1) + 0.5 = (4k + 3) / 2.
         *
         * Its reciprocal is therefore 1/a = 2 / (4k + 3). This fraction is already
         * reduced because 4k + 3 is odd.
         *
         * A rational number has a terminating decimal representation iff the
         * denominator of its reduced fraction contains no prime factors other than
         * 2 and 5. Since 4k + 3 is odd, a terminating representation would require
         * 4k + 3 to be a power of 5. However, 4k + 3 ≡ 3 (mod 4), while every power
         * of 5 is ≡ 1 (mod 4).
         *
         * Therefore 1/a always has a non-terminating repeating decimal expansion and
         * cannot be represented exactly by any finite decimal notation.
         */

        var t = new BigDec(value * sign, MaxPrecision, MaxPrecision);
        return t;
    }

    #endregion

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void AssertPrecision(int value, string fieldName = "newPrecision")
    {
        // ReSharper disable once ConvertIfStatementToSwitchStatement
        if (value < 0)
        {
            throw new InfiniteDecimalException($"Precision in variable {fieldName} is negative: {value}");
        }

        // 2^31 bits / (ln(10)/ln(2)) = 646_456_993 decimal digits
        if (value > 646_456_993)
        {
            long t = value * 10L;
            throw new OutOfMemoryException($"Awaited precision ({t:N0}) is too big");
        }
    }
}
