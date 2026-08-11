namespace BistAdvisor.Application.Indicators;

public enum SignalType
{
    StrongBuy,
    Buy,
    Neutral,
    Sell,
    StrongSell,
    InsufficientData
}

public class SignalResult
{
    public int? RsiScore { get; set; }
    public int? MacdScore { get; set; }
    public int? EmaScore { get; set; }
    public int? BollingerScore { get; set; }
    public int? StochasticScore { get; set; }
    
    public decimal? TotalScore { get; set; }
    public decimal? ConfidenceRate { get; set; }
    public SignalType? SignalType { get; set; }
}