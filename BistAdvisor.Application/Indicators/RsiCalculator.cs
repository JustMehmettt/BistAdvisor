using BistAdvisor.Domain.Entities;

namespace BistAdvisor.Application.Indicators;

public class RsiCalculator
{
    private const int Period = 14;

    public decimal? Calculate(IReadOnlyList<PriceBar> priceBars)
    {
        if (priceBars.Count < Period + 1)
        {
            return null;
        }

        var orderedBars = priceBars.OrderBy(p => p.BarTime).ToList();
        var recentBars = orderedBars.Skip(orderedBars.Count - (Period + 1)).ToList();

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

        var averageGain = totalGain / Period;
        var averageLoss = totalLoss / Period;

        if (averageLoss == 0)
        {
            return 100;
        }

        var relativeStrength = averageGain / averageLoss;
        var rsi = 100 - (100 / (1 + relativeStrength));

        return Math.Round(rsi, 2);
    }
}