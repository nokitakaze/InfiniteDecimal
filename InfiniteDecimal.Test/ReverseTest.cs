using System.Numerics;
using System.Reflection;
using Xunit.Abstractions;

namespace InfiniteDecimal.Test;

public class ReverseTest
{
    private readonly ITestOutputHelper _testOutputHelper;

    public ReverseTest(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    public static object[][] MainReverseTestData()
    {
        var values = new decimal[]
        {
            1,
            2,
            5,
            7,
            13,
            17,
            19,
            137,
            1337,
            3 * 16 * 31,
        };

        return values
            .Select(t => new object[] { t })
            .ToArray();
    }

    [Theory]
    [MemberData(nameof(MainReverseTestData))]
    public void DivisionTest(decimal input)
    {
        foreach (var precision in new[] { 5, 18, 50, 100 })
        {
            var inputBD = new BigDec(input).WithPrecision(precision + BigDec.PrecisionBuffer);
            var reverse = BigDec.One / inputBD;
            var revRev = inputBD * reverse;
            WritePrecision(revRev, precision);
            var epsilon = BigDec.PowFracOfTen(precision - 1);
            Assert.InRange(revRev, 1 - epsilon, 1 + epsilon);
        }
    }

    [Theory]
    [MemberData(nameof(MainReverseTestData))]
    public void IntPowTest(decimal input)
    {
        foreach (var precision in new[] { 5, 18, 50, 100 })
        {
            var inputBD = new BigDec(input).WithPrecision(precision + BigDec.PrecisionBuffer);
            var reverse = inputBD.Pow(-1);
            var revRev = inputBD * reverse;
            WritePrecision(revRev, precision);
            var epsilon = BigDec.PowFracOfTen(precision - 1);
            Assert.InRange(revRev, 1 - epsilon, 1 + epsilon);
        }
    }

    [Theory]
    [MemberData(nameof(MainReverseTestData))]
    public void BigDecPowTest(decimal input)
    {
        foreach (var precision in new[] { 5, 18, 50, 100 })
        {
            var inputBD = new BigDec(input).WithPrecision(precision + BigDec.PrecisionBuffer);
            var reverse = inputBD.Pow(-BigDec.One);
            var revRev = inputBD * reverse;
            WritePrecision(revRev, precision);
            var epsilon = BigDec.PowFracOfTen(precision - 1);
            Assert.InRange(revRev, 1 - epsilon, 1 + epsilon);
        }
    }

    public static object[][] BigDecPowNData()
    {
        var exps = new int[] { 2, 13, 1337 };
        return MainReverseTestData()
            .SelectMany(item => exps.Select(exp => new object[] { item[0], exp }))
            .ToArray();
    }

    [Theory]
    [MemberData(nameof(BigDecPowNData))]
    public void BigDecPowNTest(decimal input, int pow)
    {
        foreach (var precision in new[] { 5, 18, 50, 100 })
        {
            var inputBD = new BigDec(input).WithPrecision(precision + pow * BigDec.PrecisionBuffer);
            var reverse = inputBD.Pow(new BigDec(-pow));
            var revRev = inputBD.Pow(pow) * reverse;
            WritePrecision(revRev, precision);
            var epsilon = BigDec.PowFracOfTen(precision - 1);
            Assert.InRange(revRev, 1 - epsilon, 1 + epsilon);
        }
    }

    private void WritePrecision(BigDec diff, int precision)
    {
        var offsetField = typeof(BigDec)
            .GetField("_offset", BindingFlags.NonPublic | BindingFlags.Instance);
        var valueField = typeof(BigDec)
            .GetField("Value", BindingFlags.NonPublic | BindingFlags.Instance);
        if ((offsetField is null) || (valueField is null))
        {
            throw new Exception();
        }

        var t = (BigDec.One - diff).Abs();
        if (t.IsZero)
        {
            _testOutputHelper.WriteLine($"precision {precision} returns ideal");
            return;
        }

        var offset = (int)offsetField.GetValue(t)!;
        var value = (BigInteger)valueField.GetValue(t)!;
        var diffLg10 = offset - Math.Log10((double)value);
        _testOutputHelper.WriteLine($"precision {precision} returns with {diffLg10:F3} ({(diffLg10 - precision):F3})");
    }
}