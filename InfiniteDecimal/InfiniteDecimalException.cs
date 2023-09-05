using System;

namespace InfiniteDecimal;

public class InfiniteDecimalException : Exception
{
    public InfiniteDecimalException(string errMessage) : base(errMessage)
    {
    }
}