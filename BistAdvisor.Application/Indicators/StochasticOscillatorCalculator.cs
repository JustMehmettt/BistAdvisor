using BistAdvisor.Domain.Entities;

namespace BistAdvisor.Application.Indicators;

public class StochasticResult
{
    public decimal? PercentK { get; set; }
    public decimal? PercentD { get; set; }
}

public class StochasticOscillatorCalculator
{
    private const int KPeriod = 14;
    private const int DPeriod = 3;

    public StochasticResult Calculate(IReadOnlyList<PriceBar> priceBars)
    {
        var orderedBars = priceBars.OrderBy(p => p.BarTime).ToList();

        if (orderedBars.Count < KPeriod + DPeriod)
        {
            return new StochasticResult();
        }

        var kValues = new List<decimal>();

        for (var i = KPeriod - 1; i < orderedBars.Count; i++)
        {
            var window = orderedBars.Skip(i - (KPeriod - 1)).Take(KPeriod).ToList();

            var highestHigh = window.Max(p => p.HighPrice);
            var lowestLow = window.Min(p => p.LowPrice);
            var currentClose = orderedBars[i].ClosePrice;

            var range = highestHigh - lowestLow;

            var percentK = range == 0
                ? 50m
                : (currentClose - lowestLow) / range * 100;

            kValues.Add(percentK);
        }

        if (kValues.Count < DPeriod)
        {
            return new StochasticResult
            {
                PercentK = Math.Round(kValues[^1], 4)
            };
        }

        var percentD = kValues.Skip(kValues.Count - DPeriod).Take(DPeriod).Average();
        
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