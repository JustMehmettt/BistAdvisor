using BistAdvisor.Application.Indicators;
using BistAdvisor.Domain.Entities;
using Xunit;

namespace BistAdvisor.Tests.Indicators;

public class EmaTrendCalculatorTests
{
    [Fact]
    public void Calculate_WithInsufficientData_ReturnsNullValues()
    {
        var calculator = new EmaTrendCalculator();
        var priceBars = TestData.CreatePriceBars(TestData.GenerateIncreasingPrices(30));
        
        var result = calculator.Calculate(priceBars);
        
        Assert.Null(result.Ema20);
        Assert.Null(result.Ema50);
    }

    [Fact]
    public void Calculate_WithSufficientData_ReturnsDifferentEma20AndEma50()
    {
        var calculator = new EmaTrendCalculator();
        var priceBars = TestData.CreatePriceBars(TestData.GenerateIncreasingPrices(60));
        
        var result = calculator.Calculate(priceBars);

        Assert.NotNull(result.Ema20);
        Assert.NotNull(result.Ema50);
        Assert.NotEqual(result.Ema20, result.Ema50);
    }

    [Fact]
    public void Calculate_WithConsistentUptrend_Ema20IsAboveEma50()
    {
        var calculator = new EmaTrendCalculator();
        var priceBars = TestData.CreatePriceBars(TestData.GenerateIncreasingPrices(60));
        
        var result = calculator.Calculate(priceBars);
        
        Assert.True(result.Ema20 > result.Ema50);
    }
}