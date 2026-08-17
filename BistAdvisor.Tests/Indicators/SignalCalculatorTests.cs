using BistAdvisor.Application.Indicators;
using Xunit;

namespace BistAdvisor.Tests.Indicators;

public class SignalCalculatorTests
{
    
    [Fact]
    public void Calculate_WithAllMaxPositiveScores_ReturnsStrongBuy()
    {
        var calculator = new SignalCalculator();

        var result = calculator.Calculate(
            rsiScore: 2, macdScore: 2, emaScore: 2, bollingerScore: 2, stochasticScore: 2);
        
        Assert.Equal(SignalType.StrongBuy, result.SignalType);
        Assert.Equal(100, result.TotalScore);
    }
    
    [Fact]
    public void Calculate_WithAllMaxNegativeScores_ReturnsStrongSell()
    {
        var calculator = new SignalCalculator();
        var result = calculator.Calculate(
            rsiScore: -2, macdScore: -2, emaScore: -2, bollingerScore: -2, stochasticScore: -2);
        
        Assert.Equal(SignalType.StrongSell, result.SignalType);
        Assert.Equal(-100, result.TotalScore);
    }

    [Fact]
    public void Calculate_WithAllZeroScores_ReturnNeutral()
    {
        var calculator = new SignalCalculator();
        var result = calculator.Calculate(
            rsiScore: 0, macdScore: 0, emaScore: 0, bollingerScore: 0, stochasticScore: 0);
        
        Assert.Equal(SignalType.Neutral, result.SignalType);
        Assert.Equal(0, result.TotalScore);   
    }

    [Fact]
    public void Calculate_WithFewerThanFourIndicators_ReturnsInsufficientData()
    {
        var calculator = new SignalCalculator();
        var result = calculator.Calculate(
            rsiScore: 2, macdScore: 2, emaScore: null, bollingerScore: null, stochasticScore: null);
        
        Assert.Equal(SignalType.InsufficientData, result.SignalType);
        Assert.Null(result.TotalScore);
    }

    [Theory]
    [InlineData(2, 2, 2, 2, 2, SignalType.StrongBuy)]
    [InlineData(1, 1, 0, 0, 0, SignalType.Buy)]
    [InlineData(0, 0, 0, 0, 0, SignalType.Neutral)]
    [InlineData(-1, -1, 0, 0, 0, SignalType.Sell)]
    [InlineData(-2, -2, -2, -2, -2, SignalType.StrongSell)]
    public void ClassifySignal_AtThresholdBoundaries_ReturnsExpectedType(int rsiScore, int macdScore, int emaScore, int bollingerScore, int stochasticScore,
        SignalType expected)
    {
        var calculator = new SignalCalculator();
        var result = calculator.Calculate(rsiScore, macdScore, emaScore, bollingerScore, stochasticScore);
        
        Assert.Equal(expected, result.SignalType);
    }
}