namespace BistAdvisor.Application.MarketData;

public class StockListItem
{
    public string Symbol { get; set; } = string.Empty;
    public string ProviderSymbol { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string? Sector { get; set; } = string.Empty;
}