namespace BistAdvisor.Application.Indicators;

public class SignalCalculator
{
    private const decimal MacdWeight = 0.25m;
    private const decimal EmaWeight = 0.25m;
    private const decimal RsiWeight = 0.20m;
    private const decimal BollingerWeight = 0.15m;
    private const decimal StochasticWeight = 0.15m;

    private const int MinimumRequiredIndicators = 4;

    public SignalResult Calculate(
        int? rsiScore,
        int? macdScore,
        int? emaScore,
        int? bollingerScore,
        int? stochasticScore)
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
        totalScore += (rsiScore ?? 0) * RsiWeight;
        totalScore += (macdScore ?? 0) * MacdWeight;
        totalScore += (emaScore ?? 0) * EmaWeight;
        totalScore += (bollingerScore ?? 0) * BollingerWeight;
        totalScore += (stochasticScore ?? 0) * StochasticWeight;

        totalScore *= 50;

        result.TotalScore = Math.Round(totalScore, 2);
        result.SignalType = ClassifySignal(result.TotalScore.Value);
        result.ConfidenceRate = CalculateConfidence(scores, result.TotalScore.Value, availableCount);

        return result;
    }

    private static SignalType ClassifySignal(decimal totalScore)
    {
        return totalScore switch
        {
            >= 60 => SignalType.StrongBuy,
            >= 20 => SignalType.Buy,
            >= -19 => SignalType.Neutral,
            >= -59 => SignalType.Sell,
            _ => SignalType.StrongSell
        };
    }

    private static decimal CalculateConfidence(int?[] scores, decimal totalScore, int availableCount)
    {
        var direction = Math.Sign(totalScore);

        var agreeingCount = scores.Count(s => s.HasValue && Math.Sign(s.Value) == direction);
        if (direction == 0)
        {
            agreeingCount = scores.Count(s => s.HasValue && s.Value == 0);
        }

        var agreementRatio = (decimal)agreeingCount / availableCount;
        var dataCompletenessRatio = (decimal)availableCount / 5;

        var confidence = agreementRatio * 70 + dataCompletenessRatio * 15 + 15;

        return Math.Round(Math.Min(confidence, 100), 2);
    }
}