using BistAdvisor.Domain.Entities;

namespace BistAdvisor.Application.Indicators;

public class BollingerBandsResult
{
    public decimal? UpperBand { get; set; }
    public decimal? MiddleBand { get; set; }
    public decimal? LowerBand { get; set; }
    public decimal? CurrentPrice { get; set; }
}


public class BollingerBandsCalculator
{
    private readonly int _period;
    private readonly decimal _standardDeviationMultiplier;

    public BollingerBandsCalculator(int period = 20, decimal standardDeviationMultiplier = 2m)
    {
        _period = period;
        _standardDeviationMultiplier = standardDeviationMultiplier;
    }

    public BollingerBandsResult Calculate(IReadOnlyList<PriceBar> priceBars)
    {
        var orderedBars = priceBars.OrderBy(p => p.BarTime).ToList();

        if (orderedBars.Count < _period)
        {
            return new BollingerBandsResult();
        }

        var recentBars = orderedBars.Skip(orderedBars.Count - _period).ToList();
        var closePrices = recentBars.Select(p => p.ClosePrice).ToList();

        var middleBand = closePrices.Average();

        var sumOfSquaredDifferences = closePrices.Sum(price => (price - middleBand) * (price - middleBand));
        var variance = sumOfSquaredDifferences / _period;
        var standardDeviation = (decimal)Math.Sqrt((double)variance);

        var upperBand = middleBand + _standardDeviationMultiplier * standardDeviation;
        var lowerBand = middleBand - _standardDeviationMultiplier * standardDeviation;

        return new BollingerBandsResult
        {
            UpperBand = Math.Round(upperBand, 4),
            MiddleBand = Math.Round(middleBand, 4),
            LowerBand = Math.Round(lowerBand, 4),
            CurrentPrice = orderedBars[^1].ClosePrice
        };
    }

    public List<BollingerBandsResult> CalculateSeries(IReadOnlyList<PriceBar> priceBars)
    {
        var orderedBars = priceBars.OrderBy(p => p.BarTime).ToList();
        var result = new List<BollingerBandsResult>();

        for (var i = 0; i < orderedBars.Count; i++)
        {
            var window = orderedBars.Take(i + 1).ToList();
            result.Add(Calculate(window));
        }

        return result;
    }
}