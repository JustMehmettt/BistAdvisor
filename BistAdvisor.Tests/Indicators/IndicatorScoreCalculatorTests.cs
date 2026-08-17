using BistAdvisor.Application.Indicators;
using Xunit;

namespace BistAdvisor.Tests.Indicators;

public class IndicatorScoreCalculatorTests
{
    [Theory]
    [InlineData(29, 2)]
    [InlineData(35, 1)]
    [InlineData(50, 0)]
    [InlineData(65, -1)]
    [InlineData(75, -2)]
    public void ScoreRsi_AtVariousLevels_ReturnsExpectedScore(decimal rsi, int expectedScore)
    {
        var score = IndicatorScoreCalculator.ScoreRsi(rsi);

        Assert.Equal(expectedScore, score);
    }

    [Fact]
    public void ScoreRsi_WithNullValue_ReturnsNull()
    {
        var score = IndicatorScoreCalculator.ScoreRsi(null);

        Assert.Null(score);
    }

    [Fact]
    public void ScoreMacd_WithBullishCrossoverAndStrengthening_ReturnsTwo()
    {
        var macd = new MacdResult
        {
            MacdLine = 1.5m,
            SignalLine = 1.0m,
            BullishCrossover = true,
            HistogramStrengthening = true
        };

        var score = IndicatorScoreCalculator.ScoreMacd(macd);

        Assert.Equal(2, score);
    }

    [Fact]
    public void ScoreMacd_WithLineAboveSignalOnly_ReturnsOne()
    {
        var macd = new MacdResult
        {
            MacdLine = 1.5m,
            SignalLine = 1.0m,
            BullishCrossover = false,
            HistogramStrengthening = false
        };

        var score = IndicatorScoreCalculator.ScoreMacd(macd);

        Assert.Equal(1, score);
    }

    [Fact]
    public void ScoreEmaTrend_WithPriceAboveBothEmas_ReturnsTwo()
    {
        var ema = new EmaTrendResult
        {
            CurrentPrice = 110,
            Ema20 = 105,
            Ema50 = 100
        };

        var score = IndicatorScoreCalculator.ScoreEmaTrend(ema);

        Assert.Equal(2, score);
    }

    [Fact]
    public void ScoreEmaTrend_WithPriceBelowBothEmas_ReturnsMinusTwo()
    {
        var ema = new EmaTrendResult
        {
            CurrentPrice = 90,
            Ema20 = 95,
            Ema50 = 100
        };

        var score = IndicatorScoreCalculator.ScoreEmaTrend(ema);

        Assert.Equal(-2, score);
    }

    [Fact]
    public void ScoreBollinger_WithPriceAtOrBelowLowerBand_ReturnsTwo()
    {
        var bollinger = new BollingerBandsResult
        {
            CurrentPrice = 95,
            LowerBand = 95,
            MiddleBand = 100,
            UpperBand = 105
        };

        var score = IndicatorScoreCalculator.ScoreBollinger(bollinger);

        Assert.Equal(2, score);
    }

    [Fact]
    public void ScoreBollinger_WithPriceAtOrAboveUpperBand_ReturnsMinusTwo()
    {
        var bollinger = new BollingerBandsResult
        {
            CurrentPrice = 105,
            LowerBand = 95,
            MiddleBand = 100,
            UpperBand = 105
        };

        var score = IndicatorScoreCalculator.ScoreBollinger(bollinger);

        Assert.Equal(-2, score);
    }

    [Fact]
    public void ScoreStochastic_WithKAboveDAndBelowTwenty_ReturnsTwo()
    {
        var stochastic = new StochasticResult
        {
            PercentK = 15,
            PercentD = 10
        };

        var score = IndicatorScoreCalculator.ScoreStochastic(stochastic);

        Assert.Equal(2, score);
    }

    [Fact]
    public void ScoreStochastic_WithKBelowDAndAboveEighty_ReturnsMinusTwo()
    {
        var stochastic = new StochasticResult
        {
            PercentK = 85,
            PercentD = 90
        };

        var score = IndicatorScoreCalculator.ScoreStochastic(stochastic);

        Assert.Equal(-2, score);
    }
}