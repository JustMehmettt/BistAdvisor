namespace BistAdvisor.Application.Indicators;

public class SignalWeights
{
    public decimal MacdWeight { get; set; } = 0.25m;
    public decimal EmaWeight { get; set; } = 0.25m;
    public decimal RsiWeight { get; set; } = 0.20m;
    public decimal BollingerWeight { get; set; } = 0.15m;
    public decimal StochasticWeight { get; set; } = 0.15m;
    public decimal StrongBuyThreshold { get; set; } = 60m;
    public decimal BuyThreshold { get; set; } = 20m;
    public decimal NeutralThreshold { get; set; } = -19m;
    public decimal SellThreshold { get; set; } = -59m;
}

public class SignalCalculator
{
    private const int MinimumRequiredIndicators = 4;
    private readonly SignalWeights _weights;

    public SignalCalculator(SignalWeights? weights = null)
    {
        _weights = weights ?? new SignalWeights();
    }

    public SignalResult Calculate(
        int? rsiScore,
        int? macdScore,
        int? emaScore,
        int? bollingerScore,
        int? stochasticScore,
        decimal volumeRatio = 1m)
    {
        var scores = new[] { rsiScore, macdScore, emaScore, bollingerScore, stochasticScore };
        var availableCount = scores.Count(s => s.HasValue);

        var result = new SignalResult
        {
            RsiScore = rsiScore,
            MacdScore = macdScore,
            EmaScore = emaScore,
            BollingerScore = bollingerScore,
            StochasticScore = stochasticScore
        };

        if (availableCount < MinimumRequiredIndicators)
        {
            result.SignalType = SignalType.InsufficientData;
            return result;
        }

        decimal totalScore = 0;
        totalScore += (rsiScore ?? 0) * _weights.RsiWeight;
        totalScore += (macdScore ?? 0) * _weights.MacdWeight;
        totalScore += (emaScore ?? 0) * _weights.EmaWeight;
        totalScore += (bollingerScore ?? 0) * _weights.BollingerWeight;
        totalScore += (stochasticScore ?? 0) * _weights.StochasticWeight;

        totalScore *= 50;

        result.TotalScore = Math.Round(totalScore, 2);
        result.SignalType = ClassifySignal(result.TotalScore.Value);
        result.ConfidenceRate = CalculateConfidence(scores, availableCount, volumeRatio);

        return result;
    }

    private SignalType ClassifySignal(decimal totalScore)
    {
        return totalScore switch
        {
            var s when s >= _weights.StrongBuyThreshold => SignalType.StrongBuy,
            var s when s >= _weights.BuyThreshold => SignalType.Buy,
            var s when s >= _weights.NeutralThreshold => SignalType.Neutral,
            var s when s >= _weights.SellThreshold => SignalType.Sell,
            _ => SignalType.StrongSell
        };
    }

    private static decimal CalculateConfidence(int?[] scores, int availableCount, decimal volumeRatio)
    {
        var direction = Math.Sign(scores.Where(s => s.HasValue).Sum(s => s!.Value));

        var agreeingCount = scores.Count(s => s.HasValue && Math.Sign(s.Value) == direction);
        if (direction == 0)
        {
            agreeingCount = scores.Count(s => s.HasValue && s.Value == 0);
        }

        var agreementRatio = (decimal)agreeingCount / availableCount;
        var dataCompletenessRatio = (decimal)availableCount / 5;
        var clampedVolumeRatio = Math.Clamp(volumeRatio, 0m, 1m);

        var confidence = agreementRatio * 70 + dataCompletenessRatio * 15 + clampedVolumeRatio * 15;

        return Math.Round(Math.Min(confidence, 100), 2);
    }
}