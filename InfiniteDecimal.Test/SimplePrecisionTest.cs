namespace InfiniteDecimal.Test;

public class SimplePrecisionTest
{
    #region Precision copy

    [Fact]
    public void Copy_WithPrecision()
    {
        var a = new BigDec(1337);
        {
            var b1 = a.WithPrecision(a.MaxPrecision + 2);
            b1 += 1;
            Assert.NotEqual(a, b1);
        }
        {
            var b2 = a.WithPrecision(a.MaxPrecision);
            b2 += 1;
            Assert.NotEqual(a, b2);
        }
    }

    [Fact]
    public void Copy_Copy()
    {
        var a = new BigDec(1337);
        var b = a;
        b += 1;

        Assert.NotEqual(a, b);
    }

    [Fact]
    public void Copy_Round()
    {
        var a = new BigDec(1337);
        {
            var b1 = a.Round(a.MaxPrecision + 2);
            b1 += 1;
            Assert.NotEqual(a, b1);
        }
        {
            var b2 = a.Round(a.MaxPrecision);
            b2 += 1;
            Assert.NotEqual(a, b2);
        }
    }

    #endregion

    // TODO A * 0 => precision copy
}