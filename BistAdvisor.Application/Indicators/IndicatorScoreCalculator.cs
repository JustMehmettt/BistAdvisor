namespace BistAdvisor.Application.Indicators;

public static class IndicatorScoreCalculator
{
    public static int? ScoreRsi(decimal? rsi)
    {
        if (rsi is null) return null;

        return rsi switch
        {
            < 30 => 2,
            < 40 => 1,
            < 60 => 0,
            < 70 => -1,
            _ => -2
        };
    }

    public static int? ScoreMacd(MacdResult macd)
    {
        if (macd.MacdLine is null || macd.SignalLine is null) return null;

        if (macd.BullishCrossover && macd.HistogramStrengthening) return 2;
        if (macd.MacdLine > macd.SignalLine) return 1;
        if (macd.BearishCrossover && macd.HistogramStrengthening) return -2;
        if (macd.MacdLine < macd.SignalLine) return -1;
        return 0;
    }

    public static int? ScoreEmaTrend(EmaTrendResult ema)
    {
        if (ema.Ema20 is null || ema.Ema50 is null || ema.CurrentPrice is null) return null;

        var priceAboveEma20 = ema.CurrentPrice > ema.Ema20;
        var ema20AboveEma50 = ema.Ema20 > ema.Ema50;

        if (priceAboveEma20 && ema20AboveEma50) return 2;
        if (priceAboveEma20) return 1;
        if (!priceAboveEma20 && !ema20AboveEma50) return -2;
        return -1;
    }

    public static int? ScoreBollinger(BollingerBandsResult bollinger)
    {
        if (bollinger.CurrentPrice is null || bollinger.UpperBand is null ||
            bollinger.LowerBand is null || bollinger.MiddleBand is null) return null;

        var price = bollinger.CurrentPrice.Value;
        var upper = bollinger.UpperBand.Value;
        var lower = bollinger.LowerBand.Value;
        var middle = bollinger.MiddleBand.Value;

        var bandWidth = upper - lower;
        var nearThreshold = bandWidth * 0.15m;

        if (price <= lower) return 2;
        if (price <= lower + nearThreshold) return 1;
        if (price >= upper) return -2;
        if (price >= upper - nearThreshold) return -1;
        return 0;
    }

    public static int? ScoreStochastic(StochasticResult stochastic)
    {
        if (stochastic.PercentK is null || stochastic.PercentD is null) return null;

        var k = stochastic.PercentK.Value;
        var d = stochastic.PercentD.Value;

        if (k > d && k < 20) return 2;
        if (k > d) return 1;
        if (k < d && k > 80) return -2;
        if (k < d) return -1;
        return 0;
    }
}