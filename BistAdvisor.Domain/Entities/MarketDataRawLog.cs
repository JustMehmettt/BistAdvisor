namespace BistAdvisor.Domain.Entities;

public class MarketDataRawLog
{
    public long Id { get; set; }
    
    public int? StockId { get; set; }
    public Stock? Stock { get; set; }
    
    public string ProviderName { get; set; } = string.Empty;
    public string RequestSymbol { get; set; } = string.Empty;
    public string? RawResponse { get; set; }
    public bool WasSuccessful { get; set; }
    public int? RetryCount { get; set; }
    public string? ErrorMessage { get; set; }
    
    public DateTimeOffset FetchedAt { get; set; }
}