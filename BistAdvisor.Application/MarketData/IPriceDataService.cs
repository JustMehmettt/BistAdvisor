namespace BistAdvisor.Application.MarketData;

public interface IPriceDataService
{
    Task<int> SyncHistoricalDataAsync(
        string stockSymbol,
        DateTimeOffset from,
        DateTimeOffset to,
        CancellationToken cancellationToken = default);
}