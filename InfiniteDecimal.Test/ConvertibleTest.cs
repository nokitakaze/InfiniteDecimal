namespace InfiniteDecimal.Test;

public class ConvertibleTest
{
    [Fact]
    public void TestConvert()
    {
        var values = new decimal[]
        {
            -14,
            -12,
            -1,
            -0.8m,
            -0.25m,
            -0.00001m,
            0m,
            0.00001m,
            0.25m,
            0.8m,
            1,
            12,
            14,
        };

        var rnd = new Random();
        for (int i = 0; i < 100; i++)
        {
            var values1 = values
                .Select(value => new BigDec(value))
                .OrderBy(_ => rnd.NextDouble())
                .ToArray();

            var values2 = values1
                .OrderBy(t => t)
                .ToArray();
            for (int j = 1; j < values2.Length; j++)
            {
                Assert.True(values2[j - 1] < values2[j]);
            }

            values2 = values1
                .OrderByDescending(t => t)
                .ToArray();
            for (int j = 1; j < values2.Length; j++)
            {
                Assert.True(values2[j - 1] > values2[j]);
            }
        }
    }
}