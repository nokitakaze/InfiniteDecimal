using System;
using System.Numerics;
using System.Runtime.CompilerServices;

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
        var modifier = Pow10BigInt(OffsetStep);
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

        var rounded = BigDec.Rounding(value);
        Value = rounded.Value;
        Offset = rounded._offset;
        NormalizeOffset();
    }

    // TODO public BigDec(float value)

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