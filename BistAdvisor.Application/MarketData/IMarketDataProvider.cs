using BistAdvisor.Domain.Entities;

namespace BistAdvisor.Application.MarketData;

public interface IMarketDataProvider
{
    string ProviderName { get; }
    Task<IReadOnlyList<StockListItem>> GetStockListAsync(CancellationToken cancellationToken = default);
    
    Task<IReadOnlyList<MarketDataPoint>> GetHistoricalDataAsync(
        string providerSymbol,
        DateTimeOffset from,
        DateTimeOffset to,
        CancellationToken cancellationToken = default);
    
    Task<MarketDataPoint?> GetLatestPriceAsync(
        string providerSymbol, 
        CancellationToken cancellationToken = default);
    
    Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default);
}