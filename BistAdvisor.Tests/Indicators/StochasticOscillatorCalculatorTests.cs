using BistAdvisor.Application.Indicators;
using Xunit;

namespace BistAdvisor.Tests.Indicators;

public class StochasticOscillatorCalculatorTests
{
    [Fact]
    public void Calculate_WithInsufficientData_ReturnsNullValues()
    {
        var calculator = new StochasticOscillatorCalculator();
        var priceBars = TestData.CreatePriceBars(TestData.GenerateIncreasingPrices(10));

        var result = calculator.Calculate(priceBars);

        Assert.Null(result.PercentK);
        Assert.Null(result.PercentD);
    }

    [Fact]
    public void Calculate_ValuesAreWithinZeroToHundredRange()
    {
        var calculator = new StochasticOscillatorCalculator();
        var priceBars = TestData.CreatePriceBars(TestData.GenerateIncreasingPrices(30));

        var result = calculator.Calculate(priceBars);

        Assert.NotNull(result.PercentK);
        Assert.InRange(result.PercentK!.Value, 0, 100);
        Assert.NotNull(result.PercentD);
        Assert.InRange(result.PercentD!.Value, 0, 100);
    }

    [Fact]
    public void Calculate_WithConstantPrices_ReturnsNeutralFiftyPercent()
    {
        var calculator = new StochasticOscillatorCalculator();
        var constantPrices = Enumerable.Repeat(100m, 20).ToArray();
        var priceBars = TestData.CreatePriceBars(constantPrices);

        var result = calculator.Calculate(priceBars);

        Assert.Equal(50, result.PercentK);
    }
}