namespace BistAdvisor.Application.Dtos;

public class StockListItemDto
{
    public string Symbol { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string? Sector { get; set; }
    public decimal? LastPrice { get; set; }
    public decimal? DailyChangeRate { get; set; }
    public string SignalType { get; set; } = string.Empty;
    public decimal? TotalScore { get; set; }
    public decimal? ConfidenceRate { get; set; }
    public DateTimeOffset? LastUpdate { get; set; }
}