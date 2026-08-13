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

    public List<PricePointDto> PriceHistory { get; set; } = new();
    public List<SignalHistoryItemDto> SignalHistory { get; set; } = new();
}

public class PricePointDto
{
    public DateTimeOffset Date { get; set; }
    public decimal Close { get; set; }
}

public class SignalHistoryItemDto
{
    public DateTimeOffset Date { get; set; }
    public string SignalType { get; set; } = string.Empty;
    public decimal? TotalScore { get; set; }
}