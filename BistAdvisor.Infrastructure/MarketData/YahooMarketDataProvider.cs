using BistAdvisor.Application.MarketData;
using YahooFinanceApi;

namespace BistAdvisor.Infrastructure.MarketData;

public class YahooMarketDataProvider : IMarketDataProvider
{
    public async Task<IReadOnlyList<StockListItem>> GetStockListAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException("Hisse listesi ayrı bir seed/config kaynağından yönetilecektir, bu metod ilerleyen adımda dolduracaktır.");
    }

    public async Task<IReadOnlyList<MarketDataPoint>> GetHistoricalDataAsync(
        string providerSymbol,
        DateTimeOffset from,
        DateTimeOffset to,
        CancellationToken cancellationToken = default)
    {
        var candles = await Yahoo.GetHistoricalAsync(
            providerSymbol,
            from.UtcDateTime,
            to.UtcDateTime,
            Period.Daily);

        return candles
            .Select(c => new MarketDataPoint
            {
                Timestamp = new DateTimeOffset(c.DateTime, TimeSpan.Zero),
                Open = c.Open,
                High = c.High,
                Low = c.Low,
                Close = c.Close,
                AdjustedClose = c.AdjustedClose,
                Volume = c.Volume
            })
            .ToList();
    }

    public async Task<MarketDataPoint?> GetLatestPriceAsync(
        string providerSymbol,
        CancellationToken cancellationToken = default)
    {
        var quote = await Yahoo.Symbols(providerSymbol).QueryAsync();

        if (!quote.TryGetValue(providerSymbol, out var security))
        {
            return null;
        }

        return new MarketDataPoint
        {
            Timestamp = DateTimeOffset.UtcNow,
            Open = (decimal)security.RegularMarketOpen,
            High = (decimal)security.RegularMarketDayHigh,
            Low = (decimal)security.RegularMarketDayLow,
            Close = (decimal)security.RegularMarketPrice,
            AdjustedClose = null,
            Volume = (long)security.RegularMarketVolume
        };
    }

    public async Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var quote = await Yahoo.Symbols("THYAO.IS").QueryAsync();
            return quote.Count > 0;
        }
        catch
        {
            return false;
        }
    }
}