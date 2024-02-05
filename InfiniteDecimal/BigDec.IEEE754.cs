using System;
using System.Numerics;
using System.Runtime.InteropServices;

namespace InfiniteDecimal;

public partial class BigDec
{
    protected static readonly BigInteger IEEE2MantissaDenumenator;
    protected static readonly BigDec IEEE2MantissaStep;
    protected static readonly BigInteger IEEE1MantissaDenumenator;
    protected static readonly BigDec IEEE1MantissaStep;

    #region double-precision floating-point number

    /// <summary>
    /// Deconstructs a IEEE-724 double number into its sign, exponent, and mantissa components
    /// </summary>
    /// <param name="number">The double number to deconstruct</param>
    /// <returns>A tuple containing the sign, exponent, and mantissa of the double number</returns>
    public static (int sign, int exponent, ulong mantissa) DeconstructDouble(double number)
    {
        // BitConverter.DoubleToUInt64Bits still doesn't exist in netstandard 2.1
        var tempValue = new DoubleUlongUnion { DoubleValue = number };
        ulong bits = tempValue.UlongValue;

        // Extract sign
        int sign = (bits & 0x8000000000000000UL) == 0 ? 1 : -1;

        const int mantissaSize = 52;
        const ulong mantissaBit = 1UL << mantissaSize;
        const ulong mantissaMask = (mantissaBit << 1) - 1;

        // Extract the exponent and subtract the offset (1023 for double)
        int exponent = (int)((bits >> mantissaSize) & 0x7FF) - 1023;

        // Extract the mantissa and add the leading one, which is implied
        ulong mantissa = (bits & mantissaMask) | mantissaBit;

        return (sign, exponent, mantissa);
    }

    /// <summary>
    /// Rounds a double-precision floating-point number to the nearest whole number
    /// </summary>
    /// <param name="number">The number to round</param>
    /// <returns>The rounded value of the number</returns>
    public static BigDec Rounding(double number)
    {
        switch (number)
        {
            case 0:
                return BigDec.Zero;
            case 1:
                return BigDec.One;
            case -1:
                return -BigDec.One;
        }

        var (sign, exponent, mantissa) = DeconstructDouble(number);
        var powMod = new BigDec(2).WithPrecision(1023).Pow(exponent);
        var mantissaStep = IEEE2MantissaStep * powMod;
        var normalizedMantissa1 = new BigDec(mantissa).WithPrecision(1023) / IEEE2MantissaDenumenator;
        var number1 = powMod * normalizedMantissa1;

        // todo 52 is a magic number
        if (exponent >= 52)
        {
            var ret = number1 * sign;
            ret.NormalizeOffset();
            return ret;
        }

        var t1 = (int)Math.Floor(BigInteger.Log10(mantissaStep.Value));
        var t2 = BigInteger.Pow(BigInteger10, t1);
        var mantissaStep1 = mantissaStep.Copy();
        mantissaStep1.Value /= t2;
        mantissaStep1.Offset -= t1;

        while (mantissaStep1.Value >= BigInteger10)
        {
            mantissaStep1.Value /= BigInteger10;
            mantissaStep1.Offset--;
        }

        mantissaStep1.Value = BigInteger.One;

        const int slippage = 7;

        var mantissaStep1000 = mantissaStep1 * 1000;
        var floored1 = (number1 / mantissaStep1000).Floor();
        var floored0 = ((number1 - slippage * mantissaStep) / mantissaStep1000).Floor();

        if (floored0 < floored1)
        {
            // Floor
            var chiki1 = (number1 / mantissaStep1000).Floor() * mantissaStep1000;
            var ret = chiki1 * sign;
            ret.NormalizeOffset();
            return ret;
        }

        var floored2 = ((number1 + slippage * mantissaStep) / mantissaStep1000).Floor();
        if (floored2 > floored1)
        {
            // Ceiling
            return (floored2 * mantissaStep1000) * sign;
        }

        {
            // Keep this calculated number
            var ret = number1 * sign;
            ret.NormalizeOffset();
            return ret;
        }
    }

    #endregion

    #region single-precision floating-point number

    public static (int sign, int exponent, uint mantissa) DeconstructFloat(float number)
    {
        var tempValue = new FloatUintUnion { FloatValue = number };
        uint bits = tempValue.UlongValue;

        // Extract sign
        int sign = (bits & 0x80000000U) == 0 ? 1 : -1;

        const int mantissaSize = 23;
        const uint mantissaBit = 1U << mantissaSize;
        const uint mantissaMask = (mantissaBit << 1) - 1;

        // Extract the exponent and subtract the offset (127 for float)
        int exponent = (int)((bits >> mantissaSize) & 0xFF) - 127;

        // Extract the mantissa and add the leading one, which is implied
        uint mantissa = (bits & mantissaMask) | mantissaBit;

        return (sign, exponent, mantissa);
    }

    /// <summary>
    /// Rounds a single-precision floating-point number to the nearest whole number
    /// </summary>
    /// <param name="number">The number to round</param>
    /// <returns>The rounded value of the number</returns>
    public static BigDec Rounding(float number)
    {
        if (number == 0)
        {
            return BigDec.Zero;
        }

        var (sign, exponent, mantissa) = DeconstructFloat(number);
        var powMod = new BigDec(2).WithPrecision(1023).Pow(exponent);
        var mantissaStep = IEEE1MantissaStep * powMod;
        var normalizedMantissa1 = new BigDec(mantissa).WithPrecision(1023) / IEEE1MantissaDenumenator;
        var number1 = powMod * normalizedMantissa1;

        // todo 23 is a magic number
        if (exponent >= 23)
        {
            return number1 * sign;
        }

        var t1 = (int)Math.Floor(BigInteger.Log10(mantissaStep.Value));
        var t2 = BigInteger.Pow(BigInteger10, t1);
        var mantissaStep1 = mantissaStep.Copy();
        mantissaStep1.Value /= t2;
        mantissaStep1.Offset -= t1;

        while (mantissaStep1.Value >= BigInteger10)
        {
            mantissaStep1.Value /= BigInteger10;
            mantissaStep1.Offset--;
        }

        mantissaStep1.Value = BigInteger.One;

        const int slippage = 5;

        var mantissaStep1000 = mantissaStep1 * 100;
        var floored1 = (number1 / mantissaStep1000).Floor();
        var floored0 = ((number1 - slippage * mantissaStep) / mantissaStep1000).Floor();

        if (floored0 < floored1)
        {
            // Floor
            var chiki1 = (number1 / mantissaStep1000).Floor() * mantissaStep1000;
            return chiki1 * sign;
        }

        var floored2 = ((number1 + slippage * mantissaStep) / mantissaStep1000).Floor();
        if (floored2 > floored1)
        {
            // Ceiling
            return (floored2 * mantissaStep1000) * sign;
        }

        // Keep this
        return number1 * sign;
    }

    #endregion

    [StructLayout(LayoutKind.Explicit)]
    protected struct DoubleUlongUnion
    {
        [FieldOffset(0)]
        public double DoubleValue;

        [FieldOffset(0)]
        public ulong UlongValue;
    }

    [StructLayout(LayoutKind.Explicit)]
    protected struct FloatUintUnion
    {
        [FieldOffset(0)]
        public float FloatValue;

        [FieldOffset(0)]
        public uint UlongValue;
    }
}