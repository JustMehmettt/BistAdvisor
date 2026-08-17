using BistAdvisor.Application.Indicators;
using Xunit;

namespace BistAdvisor.Tests.Indicators;

public class BollingerBandsCalculatorTests
{
    [Fact]
    public void Calculate_WithInsufficientData_ReturnsNullValues()
    {
        var calculator = new BollingerBandsCalculator();
        var priceBars = TestData.CreatePriceBars(TestData.GenerateIncreasingPrices(10));

        var result = calculator.Calculate(priceBars);

        Assert.Null(result.UpperBand);
        Assert.Null(result.MiddleBand);
        Assert.Null(result.LowerBand);
    }

    [Fact]
    public void Calculate_UpperBandIsAlwaysAboveMiddleAboveLower()
    {
        var calculator = new BollingerBandsCalculator();
        var priceBars = TestData.CreatePriceBars(TestData.GenerateIncreasingPrices(30));

        var result = calculator.Calculate(priceBars);

        Assert.True(result.UpperBand > result.MiddleBand);
        Assert.True(result.MiddleBand > result.LowerBand);
    }

    [Fact]
    public void Calculate_WithConstantPrices_BandsCollapseToSameValue()
    {
        var calculator = new BollingerBandsCalculator();
        var constantPrices = Enumerable.Repeat(100m, 25).ToArray();
        var priceBars = TestData.CreatePriceBars(constantPrices);

        var result = calculator.Calculate(priceBars);

        Assert.Equal(result.MiddleBand, result.UpperBand);
        Assert.Equal(result.MiddleBand, result.LowerBand);
    }
}