namespace BistAdvisor.Application.Dtos;

public class StockDetailDto
{
    public string Symbol { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string? Sector { get; set; }
    public decimal? LastPrice { get; set; }
    public string SignalType { get; set; } = string.Empty;
    public decimal? TotalScore { get; set; }
    public decimal? ConfidenceRate { get; set; }
    public string? Explanation { get; set; }
    public DateTimeOffset? LastUpdate { get; set; }
    public string? AlgorithmVersion { get; set; }
    
    public int? RsiScore { get; set; }
    public int? MacdScore { get; set; }
    public int? EmaScore { get; set; }
    public int? BollingerScore { get; set; }
    public int? StochasticScore { get; set; }

    public List<PricePointDto> PriceHistory { get; set; } = new();
    public List<SignalHistoryItemDto> SignalHistory { get; set; } = new();
    public List<IndicatorPointDto> IndicatorHistory { get; set; } = new();
}

public class PricePointDto
{
    public DateTimeOffset Date { get; set; }
    public decimal Open { get; set; }
    public decimal High { get; set; }
    public decimal Low { get; set; }
    public decimal Close { get; set; }
}

public class SignalHistoryItemDto
{
    public DateTimeOffset Date { get; set; }
    public string SignalType { get; set; } = string.Empty;
    public decimal? TotalScore { get; set; }
}

public class IndicatorPointDto
{
    public DateTimeOffset Date { get; set; }
    public decimal? RsiValue { get; set; }
    public decimal? MacdValue { get; set; }
    public decimal? MacdSignalValue { get; set; }
    public decimal? MacdHistogramValue { get; set; }
    public decimal? Ema20 { get; set; }
    public decimal? Ema50 { get; set; }
    public decimal? BollingerUpper { get; set; }
    public decimal? BollingerMiddle { get; set; }
    public decimal? BollingerLower { get; set; }
    public decimal? StochasticK { get; set; }
    public decimal? StochasticD { get; set; }
}