using BistAdvisor.Application.Indicators;
using BistAdvisor.Domain.Entities;
using Xunit;

namespace BistAdvisor.Tests.Indicators;

public class MacdCalculatorTests
{
    [Fact]
    public void Calculate_WithInsufficientData_ReturnsNullResult()
    {
        var calculator = new MacdCalculator();
        var priceBars = CreatePriceBars(GenerateIncreasingPrices(20));

        var result = calculator.Calculate(priceBars);

        Assert.Null(result.MacdLine);
        Assert.Null(result.SignalLine);
    }

    [Fact]
    public void Calculate_WithSufficientData_ReturnsNonNullMacdLine()
    {
        var calculator = new MacdCalculator();
        var priceBars = CreatePriceBars(GenerateIncreasingPrices(40));

        var result = calculator.Calculate(priceBars);

        Assert.NotNull(result.MacdLine);
        Assert.NotNull(result.SignalLine);
        Assert.NotNull(result.Histogram);
    }

    [Fact]
    public void Calculate_HistogramEqualsLineMinusSignal()
    {
        var calculator = new MacdCalculator();
        var priceBars = CreatePriceBars(GenerateIncreasingPrices(40));

        var result = calculator.Calculate(priceBars);

        Assert.Equal(result.MacdLine!.Value - result.SignalLine!.Value, result.Histogram);
    }

    private static decimal[] GenerateIncreasingPrices(int count)
    {
        var prices = new decimal[count];
        for (var i = 0; i < count; i++)
        {
            prices[i] = 100 + i;
        }

        return prices;
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