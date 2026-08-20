namespace BistAdvisor.Domain.Entities;

public enum SignalType
{
    StrongBuy,
    Buy,
    Neutral,
    Sell,
    StrongSell,
    InsufficientData,
    StaleData,
    DataUnavailable,
    CalculationError
}

public class SignalSnapshot
{
    public long Id { get; set; }

    public int StockId { get; set; }
    public Stock Stock { get; set; } = null!;

    public DateTimeOffset BarTime { get; set; }
    public PriceInterval Interval { get; set; }

    public int? RsiScore { get; set; }
    public int? MacdScore { get; set; }
    public int? EmaScore { get; set; }
    public int? BollingerScore { get; set; }
    public int? StochasticScore { get; set; }

    public decimal? TotalScore { get; set; }
    public decimal? ConfidenceRate { get; set; }
    public SignalType SignalType { get; set; }

    public string? Explanation { get; set; }
    public string AlgorithmVersion { get; set; } = "v1.0";

    public DateTimeOffset CreatedAt { get; set; }
}