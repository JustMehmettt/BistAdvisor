using BistAdvisor.Application.MarketData;

namespace BistAdvisor.Tests.Services;

public class FailingMarketDataProvider : IMarketDataProvider
{
    private readonly int _failuresBeforeSuccess;
    private int _attemptCount;

    public string ProviderName => "FailingTestProvider";

    public FailingMarketDataProvider(int failuresBeforeSuccess)
    {
        _failuresBeforeSuccess = failuresBeforeSuccess;
    }

    public int AttemptCount => _attemptCount;

    public Task<IReadOnlyList<StockListItem>> GetStockListAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<MarketDataPoint>> GetHistoricalDataAsync(
        string providerSymbol,
        DateTimeOffset from,
        DateTimeOffset to,
        CancellationToken cancellationToken = default)
    {
        _attemptCount++;

        if (_attemptCount <= _failuresBeforeSuccess)
        {
            throw new InvalidOperationException("Simulated provider failure.");
        }

        var points = new List<MarketDataPoint>
        {
            new()
            {
                Timestamp = DateTimeOffset.UtcNow.AddDays(-1),
                Open = 100,
                High = 105,
                Low = 99,
                Close = 103,
                AdjustedClose = 103,
                Volume = 1000
            }
        };

        return Task.FromResult<IReadOnlyList<MarketDataPoint>>(points);
    }

    public Task<MarketDataPoint?> GetLatestPriceAsync(string providerSymbol, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(true);
    }
}