using System.Net.Http.Headers;
using BistAdvisor.Application.MarketData;
using BistAdvisor.Infrastructure.MarketData;

namespace BistAdvisor.Infrastructure.MarketData;

public class MockMarketDataProvider : IMarketDataProvider
{
    private static readonly Random _random = new();

    public Task<IReadOnlyList<StockListItem>> GetStockListAsync(CancellationToken cancellationToken = default)
    {
        var stocks = new List<StockListItem>
        {
            new() { Symbol = "THYAO", ProviderSymbol = "THYAO.IS", CompanyName = "Türk Hava Yolları", Sector = "Ulaştırma" },
            new() { Symbol = "AKBNK", ProviderSymbol = "AKBNK.IS", CompanyName = "Akbank", Sector = "Bankacılık" },
            new() { Symbol = "GARAN", ProviderSymbol = "GARAN.IS", CompanyName = "Garanti BBVA", Sector = "Bankacılık" }, 
            new() { Symbol = "ASELS", ProviderSymbol = "ASELS.IS", CompanyName = "Aselsan", Sector = "Savunma" },
            new() { Symbol = "SASA", ProviderSymbol = "SASA.IS", CompanyName = "Sasa Polyester", Sector = "Kimya" }
        };
        
        return Task.FromResult<IReadOnlyList<StockListItem>>(stocks);
    }

    public Task<IReadOnlyList<MarketDataPoint>> GetHistoricalDataAsync(
        string providerSymbol,
        DateTimeOffset from,
        DateTimeOffset to,
        CancellationToken cancellationToken = default)
    {
        var points = new List<MarketDataPoint>();
        var currentPrice = 100 + (decimal)_random.NextDouble() * 50;

        for (var date = from; date <= to; date = date.AddDays(1))
        {
            if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
            {
                continue;
            }
            
            var change = (decimal)(_random.NextDouble() - 0.5) * 4;
            var open = currentPrice;
            var close = Math.Max(1, open + change);
            var high = Math.Max(open, close) + (decimal)_random.NextDouble();
            var low = Math.Min(open, close) - (decimal)_random.NextDouble();
            
            points.Add(new MarketDataPoint
            {
                Timestamp = date,
                Open = Math.Round(open, 2),
                High = Math.Round(high, 2),
                Low = Math.Round(Math.Max(low, 0.01m), 2),
                Close = Math.Round(close, 2),
                AdjustedClose = Math.Round(close, 2),
                Volume = _random.Next(1_000_000, 50_000_000)
            });
            
            currentPrice = close;
        }
        
        return Task.FromResult<IReadOnlyList<MarketDataPoint>>(points);
    }

    public Task<MarketDataPoint?> GetLatestPriceAsync(
        string providerSymbol,
        CancellationToken cancellationToken = default)
    {
        var price = 100 + (decimal)_random.NextDouble() * 50;

        var point = new MarketDataPoint
        {
            Timestamp = DateTimeOffset.UtcNow,
            Open = Math.Round(price, 2),
            High = Math.Round(price + 1, 2),
            Low = Math.Round(price - 1, 2),
            Close = Math.Round(price, 2),
            AdjustedClose = Math.Round(price, 2),
            Volume = _random.Next(1_000_000, 50_000_000)
        };
        
        return Task.FromResult<MarketDataPoint?>(point);
    }

    public Task<bool> IsAvailableAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(true);
    }
}