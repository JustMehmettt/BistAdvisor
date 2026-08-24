using BistAdvisor.Domain.Entities;

namespace BistAdvisor.Application.Indicators;

public class RsiCalculator
{
    private readonly int _period;

    public RsiCalculator(int period = 14)
    {
        _period = period;
    }

    public decimal? Calculate(IReadOnlyList<PriceBar> priceBars)
    {
        if (priceBars.Count < _period + 1)
        {
            return null;
        }

        var orderedBars = priceBars.OrderBy(p => p.BarTime).ToList();
        var recentBars = orderedBars.Skip(orderedBars.Count - (_period + 1)).ToList();

        decimal totalGain = 0;
        decimal totalLoss = 0;

        for (var i = 1; i < recentBars.Count; i++)
        {
            var change = recentBars[i].ClosePrice - recentBars[i - 1].ClosePrice;

            if (change > 0)
            {
                totalGain += change;
            }
            else
            {
                totalLoss += Math.Abs(change);
            }
        }

        var averageGain = totalGain / _period;
        var averageLoss = totalLoss / _period;

        if (averageLoss == 0)
        {
            return 100;
        }

        var relativeStrength = averageGain / averageLoss;
        var rsi = 100 - (100 / (1 + relativeStrength));

        return Math.Round(rsi, 2);
    }

    public List<decimal?> CalculateSeries(IReadOnlyList<PriceBar> priceBars)
    {
        var orderedBars = priceBars.OrderBy(p => p.BarTime).ToList();
        var result = new List<decimal?>();

        for (var i = 0; i < orderedBars.Count; i++)
        {
            var window = orderedBars.Take(i + 1).ToList();
            result.Add(Calculate(window));
        }

        return result;
    }
}