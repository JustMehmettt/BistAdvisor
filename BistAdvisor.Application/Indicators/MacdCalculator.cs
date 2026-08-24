using BistAdvisor.Domain.Entities;

namespace BistAdvisor.Application.Indicators;

public class MacdResult
{
    public decimal? MacdLine { get; set; }
    public decimal? SignalLine { get; set; }
    public decimal? Histogram { get; set; }
    public bool BullishCrossover { get; set; }
    public bool BearishCrossover { get; set; }
    public bool HistogramStrengthening { get; set; }
}

public class MacdCalculator
{
    private readonly int _fastPeriod;
    private readonly int _slowPeriod;
    private readonly int _signalPeriod;

    public MacdCalculator(int fastPeriod = 12, int slowPeriod = 26, int signalPeriod = 9)
    {
        _fastPeriod = fastPeriod;
        _slowPeriod = slowPeriod;
        _signalPeriod = signalPeriod;
    }

    public MacdResult Calculate(IReadOnlyList<PriceBar> priceBars)
    {
        var orderedBars = priceBars.OrderBy(p => p.BarTime).ToList();

        if (orderedBars.Count < _slowPeriod + _signalPeriod)
        {
            return new MacdResult();
        }

        var closePrices = orderedBars.Select(p => p.ClosePrice).ToList();

        var fastEma = EmaCalculator.CalculateSeries(closePrices, _fastPeriod);
        var slowEma = EmaCalculator.CalculateSeries(closePrices, _slowPeriod);

        var offset = fastEma.Count - slowEma.Count;

        var macdLineSeries = new List<decimal>();
        for (var i = 0; i < slowEma.Count; i++)
        {
            macdLineSeries.Add(fastEma[i + offset] - slowEma[i]);
        }

        var signalSeries = EmaCalculator.CalculateSeries(macdLineSeries, _signalPeriod);

        if (signalSeries.Count == 0)
        {
            return new MacdResult
            {
                MacdLine = Math.Round(macdLineSeries[^1], 4)
            };
        }

        var macdLine = macdLineSeries[^1];
        var signalLine = signalSeries[^1];
        var histogram = macdLine - signalLine;

        var bullishCrossover = false;
        var bearishCrossover = false;
        var histogramStrengthening = false;

        if (signalSeries.Count >= 2 && macdLineSeries.Count >= 2)
        {
            var previousMacd = macdLineSeries[^2];
            var previousSignal = signalSeries[^2];
            var previousHistogram = previousMacd - previousSignal;

            bullishCrossover = previousMacd <= previousSignal && macdLine > signalLine;
            bearishCrossover = previousMacd >= previousSignal && macdLine < signalLine;
            histogramStrengthening = Math.Abs(histogram) > Math.Abs(previousHistogram);
        }

        return new MacdResult
        {
            MacdLine = Math.Round(macdLine, 4),
            SignalLine = Math.Round(signalLine, 4),
            Histogram = Math.Round(histogram, 4),
            BullishCrossover = bullishCrossover,
            BearishCrossover = bearishCrossover,
            HistogramStrengthening = histogramStrengthening
        };
    }

    public List<MacdResult> CalculateSeries(IReadOnlyList<PriceBar> priceBars)
    {
        var orderedBars = priceBars.OrderBy(p => p.BarTime).ToList();
        var result = new List<MacdResult>();

        for (var i = 0; i < orderedBars.Count; i++)
        {
            var window = orderedBars.Take(i + 1).ToList();
            result.Add(Calculate(window));
        }

        return result;
    }
}