using BistAdvisor.Domain.Entities;

namespace BistAdvisor.Tests;

public class TestData
{
    public static decimal[] GenerateIncreasingPrices(int count, decimal start = 100)
    {
        var prices = new decimal[count];
        for (var i = 0; i < count; i++)
        {
            prices[i] = start + i;
        }
        return prices;
    }

    public static List<PriceBar> CreatePriceBars(decimal[] closePrices)
    {
        var bars = new List<PriceBar>();
        var startDate = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

        for (var i = 0; i < closePrices.Length; i++)
        {
            bars.Add(new PriceBar
            {
                BarTime = startDate.AddDays(i),
                ClosePrice = closePrices[i],
                OpenPrice = closePrices[i],
                HighPrice = closePrices[i],
                LowPrice = closePrices[i],
                Volume = 1000
            });
        }

        return bars;
    }
}