using System;
using System.Globalization;
using System.Numerics;

namespace InfiniteDecimal;

public partial class BigDec : System.IConvertible
{
    public TypeCode GetTypeCode()
    {
        return TypeCode.Object;
    }

    public bool ToBoolean(IFormatProvider provider)
    {
        return !this.Mantissa.IsZero;
    }

    public char ToChar(IFormatProvider provider)
    {
        throw new InfiniteDecimalException("Can't type cast BigDec to char");
    }

    public DateTime ToDateTime(IFormatProvider provider)
    {
        throw new InfiniteDecimalException("Can't type cast BigDec to DateTime");
    }

    public decimal ToDecimal(IFormatProvider provider)
    {
        return (decimal)this;
    }

    public double ToDouble(IFormatProvider provider)
    {
        return (double)this;
    }

    public float ToSingle(IFormatProvider provider)
    {
        return (float)(double)this;
    }

    public ulong ToUInt64(IFormatProvider provider)
    {
        // rounded to the nearest N-bit integer
        var value = (BigInteger)this.Round(0);
        if (value < ulong.MinValue || value > ulong.MaxValue)
        {
            throw new OverflowException($"Value {value:N0} is out of range for uint64 (ulong)");
        }

        return (ulong)value;
    }

    public long ToInt64(IFormatProvider provider)
    {
        // rounded to the nearest N-bit integer
        var value = (BigInteger)this.Round(0);
        if (value < long.MinValue || value > long.MaxValue)
        {
            throw new OverflowException($"Value {value:N0} is out of range for int64 (long)");
        }

        return (long)value;
    }

    public uint ToUInt32(IFormatProvider provider)
    {
        // rounded to the nearest N-bit integer
        return Convert.ToUInt32(this.ToUInt64(provider));
    }

    public int ToInt32(IFormatProvider provider)
    {
        // rounded to the nearest N-bit integer
        return Convert.ToInt32(this.ToInt64(provider));
    }

    public ushort ToUInt16(IFormatProvider provider)
    {
        // rounded to the nearest N-bit integer
        return Convert.ToUInt16(this.ToUInt64(provider));
    }

    public short ToInt16(IFormatProvider provider)
    {
        // rounded to the nearest N-bit integer
        return Convert.ToInt16(this.ToInt64(provider));
    }

    public sbyte ToSByte(IFormatProvider provider)
    {
        // rounded to the nearest N-bit integer
        return Convert.ToSByte(this.ToInt64(provider));
    }

    public byte ToByte(IFormatProvider provider)
    {
        return Convert.ToByte(this.ToUInt64(provider));
    }

    public object ToType(Type conversionType, IFormatProvider? provider)
    {
        provider ??= CultureInfo.InvariantCulture;
        if (conversionType == typeof(ulong))
        {
            return this.ToUInt64(provider);
        }
        else if (conversionType == typeof(long))
        {
            return this.ToInt64(provider);
        }
        else if (conversionType == typeof(uint))
        {
            return this.ToUInt32(provider);
        }
        else if (conversionType == typeof(int))
        {
            return this.ToInt32(provider);
        }
        else if (conversionType == typeof(ushort))
        {
            return this.ToUInt16(provider);
        }
        else if (conversionType == typeof(short))
        {
            return this.ToInt16(provider);
        }
        else if (conversionType == typeof(byte))
        {
            return this.ToByte(provider);
        }
        else if (conversionType == typeof(sbyte))
        {
            return this.ToSByte(provider);
        }
        else if (conversionType == typeof(decimal))
        {
            return this.ToDecimal(provider);
        }
        else if (conversionType == typeof(double))
        {
            return this.ToDouble(provider);
        }
        else if (conversionType == typeof(float))
        {
            return this.ToSingle(provider);
        }
        else if (conversionType == typeof(string))
        {
            return this.ToString(provider);
        }
        else if (conversionType == typeof(BigInteger))
        {
            return (BigInteger)this;
        }
        else if (conversionType == typeof(bool))
        {
            return this.ToBoolean(provider);
        }
        else
        {
            throw new InfiniteDecimalException($"Can't convert to type '{conversionType.FullName}'");
        }
    }
}
