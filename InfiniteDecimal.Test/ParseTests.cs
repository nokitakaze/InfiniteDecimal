using System.Numerics;

namespace InfiniteDecimal.Test;

public class ParseTests
{
    [Fact]
    public void ParseUntrimmed()
    {
        //
        const decimal value1 = 13.37m;
        var expected = new BigDec(value1);
        var actual1 = BigDec.Parse("13.370000");
        Assert.True(expected == actual1);
        Assert.False(expected != actual1);
        Assert.Equal(expected, actual1);

        //
        const decimal value2 = 15m;
        expected = new BigDec(value2);
        actual1 = BigDec.Parse("15.00000");
        Assert.True(expected == actual1);
        Assert.False(expected != actual1);
        Assert.Equal(expected, actual1);

        var actual2 = BigDec.Parse("15.");
        Assert.True(expected == actual2);
        Assert.False(expected != actual2);
        Assert.Equal(expected, actual2);
    }

    #region Parse E

    public static object[][] ParseEData()
    {
        var result = new List<(string input, BigDec expected)>()
        {
            ("0e+0", BigDec.Zero),
            ("+0e+0", BigDec.Zero),
            ("-0e+0", BigDec.Zero),
            ("0e-0", BigDec.Zero),
            ("+0e-0", BigDec.Zero),
            ("-0e-0", BigDec.Zero),
            ("0.e+0", BigDec.Zero),
            ("+0.e+0", BigDec.Zero),
            ("-0.e+0", BigDec.Zero),
            ("0.e-0", BigDec.Zero),
            ("+0.e-0", BigDec.Zero),
            ("-0.e-0", BigDec.Zero),
            ("0.0e+0", BigDec.Zero),
            ("+0.0e+0", BigDec.Zero),
            ("-0.0e+0", BigDec.Zero),
            ("0.0e-0", BigDec.Zero),
            ("+0.0e-0", BigDec.Zero),
            ("-0.0e-0", BigDec.Zero),
            ("-13.e+0", new BigDec(-13)),
            ("-13.e-0", new BigDec(-13)),
            ("13.e+1", new BigDec(130)),
            ("13.37e+1", new BigDec(1337) / 10),
        };

        var rnd = new Random();
        foreach (var length in new[] { 1, 4, 16, 32, 64 })
        {
            var bytes = new byte[length];
            rnd.NextBytes(bytes);

            var t = new BigInteger(bytes, isBigEndian: true, isUnsigned: true);
            var s = t.ToString();
            var len1 = s.Length - 1;
            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var sign in new[] { 1, -1 })
            {
                // ReSharper disable once LoopCanBeConvertedToQuery
                foreach (var offset in new[] { -100, -1, 0, 1, 100 })
                {
                    var inputString = string.Format(
                        "{0}{1}.{2}e{3}",
                        (sign == 1) ? "" : "-",
                        s[..1],
                        s[1..],
                        (offset < 0) ? offset.ToString() : "+" + offset.ToString()
                    );

                    var offset1 = offset - len1;
                    var expected = t * BigDec.PowFracOfTen(-offset1) * sign;
                    result.Add((inputString, expected));
                }
            }
        }

        return result
            .Select(t => new object[] { t.input, t.expected })
            .ToArray();
    }

    [Theory]
    [MemberData(nameof(ParseEData))]
    public void ParseE(string input, BigDec expected)
    {
        var actual = BigDec.Parse(input);
        Assert.Equal(expected, actual);
        Assert.True(expected == actual);
        Assert.False(expected != actual);
    }

    #endregion
}