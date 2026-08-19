using BistAdvisor.Application.MarketData;
using BistAdvisor.Domain.Entities;
using BistAdvisor.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BistAdvisor.Infrastructure.MarketData;

public class PriceDataService : IPriceDataService
{
    private readonly ApplicationDbContext _context;
    private readonly IMarketDataProvider _marketDataProvider;

    public PriceDataService(ApplicationDbContext context, IMarketDataProvider marketDataProvider)
    {
        _context = context;
        _marketDataProvider = marketDataProvider;
    }

    public async Task<int> SyncHistoricalDataAsync(
        string stockSymbol,
        DateTimeOffset from,
        DateTimeOffset to,
        CancellationToken cancellationToken = default)
    {
        var stock = await _context.Stocks
            .FirstOrDefaultAsync(s => s.Symbol == stockSymbol, cancellationToken);

        if (stock is null)
        {
            throw new InvalidOperationException($"'{stockSymbol}' sembollü hisse veritabanında bulunamadı.");
        }

        var points = await _marketDataProvider.GetHistoricalDataAsync(
            stock.ProviderSymbol,
            from,
            to,
            cancellationToken);

        var existingBarTimes = await _context.PriceBars
            .Where(p => p.StockId == stock.Id && p.Interval == PriceInterval.Daily)
            .Select(p => p.BarTime)
            .ToListAsync(cancellationToken);

        var existingSet = existingBarTimes.ToHashSet();

        var newBars = new List<PriceBar>();
        var now = DateTimeOffset.UtcNow;

        foreach (var point in points)
        {
            var normalizedBarTime = new DateTimeOffset(point.Timestamp.Date, TimeSpan.Zero);

            if (existingSet.Contains(normalizedBarTime))
            {
                continue;
            }

            if (point.Close <= 0 || point.Open <= 0 || point.High <= 0 || point.Low <= 0)
            {
                continue;
            }

            newBars.Add(new PriceBar
            {
                StockId = stock.Id,
                Interval = PriceInterval.Daily,
                BarTime = normalizedBarTime,
                OpenPrice = point.Open,
                HighPrice = point.High,
                LowPrice = point.Low,
                ClosePrice = point.Close,
                AdjustedClosePrice = point.AdjustedClose,
                Volume = point.Volume,
                DataSource = _marketDataProvider.ProviderName,
                ReceivedAt = now,
                CreatedAt = now
            });
        }

        if (newBars.Count > 0)
        {
            await _context.PriceBars.AddRangeAsync(newBars, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        return newBars.Count;
    }
}