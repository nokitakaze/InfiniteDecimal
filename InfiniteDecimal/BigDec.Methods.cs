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
}