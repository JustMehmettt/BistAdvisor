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
    private const int Period = 20;
    private const decimal StandardDeviationMultiplier = 2m;

    public BollingerBandsResult Calculate(IReadOnlyList<PriceBar> priceBars)
    {
        var orderedBars = priceBars.OrderBy(p => p.BarTime).ToList();

        if (orderedBars.Count < Period)
        {
            return new BollingerBandsResult();
        }

        var recentBars = orderedBars.Skip(orderedBars.Count - Period).ToList();
        var closePrices = recentBars.Select(p => p.ClosePrice).ToList();

        var middleBand = closePrices.Average();

        var sumOfSquareDifferences = closePrices.Sum(price => (price - middleBand) * (price - middleBand));
        var variance = sumOfSquareDifferences / Period;
        var standardDeviation = (decimal)Math.Sqrt((double)variance);

        var upperBand = middleBand + StandardDeviationMultiplier * standardDeviation;
        var lowerBand = middleBand - StandardDeviationMultiplier * standardDeviation;
        
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