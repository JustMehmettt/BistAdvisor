namespace BistAdvisor.Application.Indicators;

public static class EmaCalculator
{
    public static List<decimal> CalculateSeries(IReadOnlyList<decimal> values, int period)
    {
        var result = new List<decimal>();

        if (values.Count < period)
        {
            return result;
        }
        
        var multiplier = 2m / (period + 1);
        
        var sma = values.Take(period).Average();
        result.Add(sma);

        for (var i = period; i < values.Count; i++)
        {
            var ema = (values[i] - result[^1]) * multiplier + result[^1];
            result.Add(ema);
        }

        return result;
    }
}