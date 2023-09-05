using System;
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
        return (this > Zero);
    }

    public byte ToByte(IFormatProvider provider)
    {
        return (byte)(ulong)this;
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

    public short ToInt16(IFormatProvider provider)
    {
        return (short)(long)this;
    }

    public int ToInt32(IFormatProvider provider)
    {
        return (int)this;
    }

    public long ToInt64(IFormatProvider provider)
    {
        return (long)this;
    }

    public sbyte ToSByte(IFormatProvider provider)
    {
        return (sbyte)(int)this;
    }

    public float ToSingle(IFormatProvider provider)
    {
        return (float)(double)this;
    }

    public object ToType(Type conversionType, IFormatProvider provider)
    {
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
        else
        {
            throw new InfiniteDecimalException($"Can't convert to type '{conversionType.FullName}'");
        }
    }

    public ushort ToUInt16(IFormatProvider provider)
    {
        return (ushort)(ulong)this;
    }

    public uint ToUInt32(IFormatProvider provider)
    {
        return (uint)(ulong)this;
    }

    public ulong ToUInt64(IFormatProvider provider)
    {
        return (ulong)this;
    }
}