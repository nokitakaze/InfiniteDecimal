using System.Globalization;

namespace InfiniteDecimal.CodeGenerator;

public static class DecFloatPrecision
{
    public static Task Test()
    {
        var m0 = 0.1m;
        for (var i0 = 0; i0 < 7; i0++)
        {
            m0 *= 10m;
            var m1 = 1m;
            for (var i1 = 1; i1 < 7; i1++)
            {
                m1 *= 0.1m;
                for (var i1a = 1; i1a <= 6; i1a++)
                {
                    var full = m0 + m1 * i1a;

                    //
                    var confDouble = (double)full;
                    var viaDouble = (decimal)confDouble;
                    var confFloat = (float)full;
                    var viaFloat = (decimal)confFloat;

                    var parsedDouble = BigDec.Parse(confDouble.ToString("G", CultureInfo.InvariantCulture));
                    var parsedFloat = BigDec.Parse(confFloat.ToString("G", CultureInfo.InvariantCulture));

                    var diffDouble = (parsedDouble - full).Abs();
                    if (diffDouble > 0)
                    {
                        Console.WriteLine(
                            "d\t{0}\t{1}\t{2}\t{3}",
                            i0,
                            i1,
                            i1a,
                            diffDouble
                        );
                    }

                    var diffFloat = (parsedFloat - full).Abs();
                    if (diffFloat > 0)
                    {
                        Console.WriteLine(
                            "f\t{0}\t{1}\t{2}\t{3}",
                            i0,
                            i1,
                            i1a,
                            diffFloat
                        );
                    }
                }
            }
        }

        return Task.CompletedTask;
    }
}