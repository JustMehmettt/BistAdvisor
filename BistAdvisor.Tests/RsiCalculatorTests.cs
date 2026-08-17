using BistAdvisor.Application.Indicators;
using BistAdvisor.Domain.Entities;
using Xunit;

namespace BistAdvisor.Tests.Indicators;

public class RsiCalculatorTests
{
    [Fact]
    public void Calculate_WithInsufficientData_ReturnsNull()
    {
        var calculator = new RsiCalculator();
        var priceBars = CreatePriceBars(new decimal[] { 100, 101, 102});
        
        var result = calculator.Calculate(priceBars);
        
        Assert.Null(result);
    }

    [Fact]
    public void Calculate_WithConsistentGains_ReturnsHighRsi()
    {
        var calculator = new RsiCalculator();

        var prices = new decimal[15];
        for (var i = 0; i < prices.Length; i++)
        {
            prices[i] = 100 + i;
        }
        
        var priceBars = CreatePriceBars(prices);
        
        var result = calculator.Calculate(priceBars);
        
        Assert.NotNull(result);
        Assert.Equal(100, result);
    }

    private static List<PriceBar> CreatePriceBars(decimal[] closePrices)
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