using BistAdvisor.Application.MarketData;
using OoplesFinance.YahooFinanceAPI;
using OoplesFinance.YahooFinanceAPI.Enums;

namespace BistAdvisor.Infrastructure.MarketData;

public class YahooMarketDataProvider : IMarketDataProvider
{
    private readonly YahooClient _client = new();
    public string ProviderName => "Yahoo";

    public Task<IReadOnlyList<StockListItem>> GetStockListAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException(
            "Hisse listesi ayrı bir seed/config kaynağından yönetilecek, bu metod ilerleyen adımda dolduracağız.");
    }

    public async Task<IReadOnlyList<MarketDataPoint>> GetHistoricalDataAsync(
        string providerSymbol,
        DateTimeOffset from,
        DateTimeOffset to,
        CancellationToken cancellationToken = default)
    {
        var historicalData = await _client.GetHistoricalDataAsync(
            providerSymbol,
            DataFrequency.Daily,
            from.UtcDateTime);

        return historicalData
            .Where(d => d.Date <= to.UtcDateTime)
            .Select(d => new MarketDataPoint
            {
                Timestamp = new DateTimeOffset(d.Date, TimeSpan.Zero),
                Open = (decimal)d.Open,
                High = (decimal)d.High,
                Low = (decimal)d.Low,
                Close = (decimal)d.Close,
                AdjustedClose = (decimal)d.AdjustedClose,
                Volume = d.Volume
            })
            .ToList();
    }

    public async Task<MarketDataPoint?> GetLatestPriceAsync(
        string providerSymbol,
        CancellationToken cancellationToken = default)
    {
        var historicalData = await _client.GetHistoricalDataAsync(
            providerSymbol,
            DataFrequency.Daily,
            DateTime.UtcNow.AddDays(-5));

        var latest = historicalData.LastOrDefault();

        if (latest is null)
        {
            return null;
        }

        return new MarketDataPoint
        {
            Timestamp = new DateTimeOffset(latest.Date, TimeSpan.Zero),
            Open = (decimal)latest.Open,
            High = (decimal)latest.High,
            Low = (decimal)latest.Low,
            Close = (decimal)latest.Close,
            AdjustedClose = (decimal)latest.AdjustedClose,
            Volume = latest.Volume
        };
    }

    public async Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var data = await _client.GetHistoricalDataAsync(
                "THYAO.IS",
                DataFrequency.Daily,
                DateTime.UtcNow.AddDays(-5));

            return data.Any();
        }
        catch
        {
            return false;
        }
    }
}