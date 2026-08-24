using BistAdvisor.Domain.Entities;

namespace BistAdvisor.Application.Indicators;

public class StochasticResult
{
    public decimal? PercentK { get; set; }
    public decimal? PercentD { get; set; }
}

public class StochasticOscillatorCalculator
{
    private readonly int _kPeriod;
    private readonly int _dPeriod;

    public StochasticOscillatorCalculator(int kPeriod = 14, int dPeriod = 3)
    {
        _kPeriod = kPeriod;
        _dPeriod = dPeriod;
    }

    public StochasticResult Calculate(IReadOnlyList<PriceBar> priceBars)
    {
        var orderedBars = priceBars.OrderBy(p => p.BarTime).ToList();

        if (orderedBars.Count < _kPeriod + _dPeriod)
        {
            return new StochasticResult();
        }

        var kValues = new List<decimal>();

        for (var i = _kPeriod - 1; i < orderedBars.Count; i++)
        {
            var window = orderedBars.Skip(i - (_kPeriod - 1)).Take(_kPeriod).ToList();

            var highestHigh = window.Max(p => p.HighPrice);
            var lowestLow = window.Min(p => p.LowPrice);
            var currentClose = orderedBars[i].ClosePrice;

            var range = highestHigh - lowestLow;

            var percentK = range == 0
                ? 50m
                : (currentClose - lowestLow) / range * 100;

            kValues.Add(percentK);
        }

        if (kValues.Count < _dPeriod)
        {
            return new StochasticResult
            {
                PercentK = Math.Round(kValues[^1], 4)
            };
        }

        var percentD = kValues.Skip(kValues.Count - _dPeriod).Take(_dPeriod).Average();

        return new StochasticResult
        {
            PercentK = Math.Round(kValues[^1], 4),
            PercentD = Math.Round(percentD, 4)
        };
    }

    public List<StochasticResult> CalculateSeries(IReadOnlyList<PriceBar> priceBars)
    {
        var orderedBars = priceBars.OrderBy(p => p.BarTime).ToList();
        var result = new List<StochasticResult>();

        for (var i = 0; i < orderedBars.Count; i++)
        {
            var window = orderedBars.Take(i + 1).ToList();
            result.Add(Calculate(window));
        }

        return result;
    }
}