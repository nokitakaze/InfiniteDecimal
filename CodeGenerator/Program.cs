namespace InfiniteDecimal.CodeGenerator;

internal static class Program
{
    private static Task Main(string[] argv)
    {
        int mode = 0;
        if (argv.Length > 0)
        {
            mode = int.Parse(argv[0]);
        }

        return mode switch
        {
            0 => BigDecConstructor.Generate(),
            1 => PowDecPrecision.CalculatePowDec(),
            2 => DecFloatPrecision.Test(),
            3 => EConstants.CalculateESquareRoots(),
            _ => throw new Exception($"Mode '{mode}' is not found")
        };
    }
}