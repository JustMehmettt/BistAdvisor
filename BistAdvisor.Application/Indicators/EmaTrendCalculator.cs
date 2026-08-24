using BistAdvisor.Domain.Entities;

namespace BistAdvisor.Application.Indicators;

public class EmaTrendResult
{
    public decimal? Ema20{ get; set; }
    public decimal? Ema50{ get; set; }
    public decimal? CurrentPrice { get; set; }
    public bool GoldenCross { get; set; }
    public bool DeathCross { get; set; }
}

public class EmaTrendCalculator
{
    private readonly int _shortPeriod;
    private readonly int _longPeriod;

    public EmaTrendCalculator(int shortPeriod = 20, int longPeriod = 50)
    {
        _shortPeriod = shortPeriod;
        _longPeriod = longPeriod;
    }

    public EmaTrendResult Calculate(IReadOnlyList<PriceBar> priceBars)
    {
        var orderedBars = priceBars.OrderBy(p => p.BarTime).ToList();

        if (orderedBars.Count < _longPeriod)
        {
            return new EmaTrendResult();
        }

        var closePrices = orderedBars.Select(p => p.ClosePrice).ToList();

        var ema20Series = EmaCalculator.CalculateSeries(closePrices, _shortPeriod);
        var ema50Series = EmaCalculator.CalculateSeries(closePrices, _longPeriod);

        var currentEma20 = ema20Series[^1];
        var currentEma50 = ema50Series[^1];
        var currentPrice = closePrices[^1];

        var goldenCross = false;
        var deathCross = false;

        if (ema50Series.Count >= 2)
        {
            var previousEma20 = ema20Series[^2];
            var previousEma50 = ema50Series[^2];

            goldenCross = previousEma20 <= previousEma50 && currentEma20 > currentEma50;
            deathCross = previousEma20 >= previousEma50 && currentEma20 < currentEma50;
        }

        return new EmaTrendResult
        {
            Ema20 = Math.Round(currentEma20, 4),
            Ema50 = Math.Round(currentEma50, 4),
            CurrentPrice = currentPrice,
            GoldenCross = goldenCross,
            DeathCross = deathCross
        };
    }
}