using System.Globalization;
using BistAdvisor.Application.Bulletins;
using BistAdvisor.Domain.Entities;
using BistAdvisor.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BistAdvisor.Infrastructure.Bulletins;

public class BulletinService : IBulletinService
{
    private readonly ApplicationDbContext _context;

    public BulletinService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DailyBulletin> GenerateDailyBulletinAsync(
        DateOnly bulletinDate,
        CancellationToken cancellationToken = default)
    {
        var existingActive = await _context.DailyBulletins
            .FirstOrDefaultAsync(
                b => b.BulletinDate == bulletinDate && b.Status == BulletinStatus.Active,
                cancellationToken);

        if (existingActive is not null)
        {
            existingActive.Status = BulletinStatus.Revised;
        }

        var stocks = await _context.Stocks.Where(s => s.IsActive).ToListAsync(cancellationToken);

        var allSignals = await _context.SignalSnapshots.ToListAsync(cancellationToken);
        var latestSignals = allSignals
            .GroupBy(s => s.StockId)
            .Select(g => g.OrderByDescending(s => s.CreatedAt).First())
            .ToDictionary(s => s.StockId);

        var allPrices = await _context.PriceBars
            .Where(p => p.Interval == PriceInterval.Daily)
            .ToListAsync(cancellationToken);
        var pricesByStock = allPrices
            .GroupBy(p => p.StockId)
            .ToDictionary(g => g.Key, g => g.OrderBy(p => p.BarTime).ToList());

        var allChanges = await _context.SignalChanges
            .Where(c => c.ChangeTime >= bulletinDate.ToDateTime(TimeOnly.MinValue))
            .Select(c => c.StockId)
            .ToListAsync(cancellationToken);
        var changedStockIds = allChanges.ToHashSet();

        var items = new List<BulletinItem>();
        var rank = 1;
        var now = DateTimeOffset.UtcNow;

        var relevantStocks = stocks
            .Where(s => latestSignals.ContainsKey(s.Id))
            .Select(s => new { Stock = s, Signal = latestSignals[s.Id] })
            .Where(x =>
                x.Signal.SignalType is SignalType.StrongBuy or SignalType.Buy
                    or SignalType.Sell or SignalType.StrongSell
                || changedStockIds.Contains(x.Stock.Id))
            .OrderByDescending(x => x.Signal.TotalScore)
            .ToList();

        foreach (var entry in relevantStocks)
        {
            var priceHistory = pricesByStock.GetValueOrDefault(entry.Stock.Id) ?? new List<PriceBar>();
            var lastPrice = priceHistory.Count > 0 ? priceHistory[^1].ClosePrice : (decimal?)null;
            var previousPrice = priceHistory.Count > 1 ? priceHistory[^2].ClosePrice : (decimal?)null;

            decimal? dailyChangeRate = null;
            if (lastPrice.HasValue && previousPrice.HasValue && previousPrice.Value != 0)
            {
                dailyChangeRate = Math.Round((lastPrice.Value - previousPrice.Value) / previousPrice.Value * 100, 2);
            }

            items.Add(new BulletinItem
            {
                StockId = entry.Stock.Id,
                Rank = rank++,
                SignalType = entry.Signal.SignalType,
                TotalScore = entry.Signal.TotalScore,
                ConfidenceRate = entry.Signal.ConfidenceRate,
                LastPrice = lastPrice,
                DailyChangeRate = dailyChangeRate,
                ReasonText = BuildReasonText(entry.Stock.Symbol, entry.Signal),
                CreatedAt = now
            });
        }

        var strongBuyCount = items.Count(i => i.SignalType == SignalType.StrongBuy);
        var buyCount = items.Count(i => i.SignalType == SignalType.Buy);
        var sellCount = items.Count(i => i.SignalType == SignalType.Sell);
        var strongSellCount = items.Count(i => i.SignalType == SignalType.StrongSell);
        var changedCount = changedStockIds.Count; 

        var summary = $"{strongBuyCount} strong buy, {buyCount} buy, {sellCount} sell, {strongSellCount} strong sell signals. " +
                      $"{changedCount} stocks changed signal today.";

        var content = BuildBulletinContent(bulletinDate, items, strongBuyCount, buyCount, sellCount, strongSellCount);

        var bulletin = new DailyBulletin
        {
            BulletinDate = bulletinDate,
            Title = $"Daily Technical Analysis Bulletin - {bulletinDate:dd.MM.yyyy}",
            Summary = summary,
            Content = content,
            Status = BulletinStatus.Active,
            GeneratedAt = now,
            PublishedAt = now,
            AlgorithmVersion = "v1.0",
            Items = items
        };

        _context.DailyBulletins.Add(bulletin);
        await _context.SaveChangesAsync(cancellationToken);

        return bulletin;
    }

    private static string BuildReasonText(string symbol, SignalSnapshot signal)
    {
        var score = signal.TotalScore?.ToString("F2", CultureInfo.InvariantCulture) ?? "-";
        var confidence = signal.ConfidenceRate?.ToString("F1", CultureInfo.InvariantCulture) ?? "-";

        return $"{symbol} generated a {signal.SignalType} signal with a technical score of {score} " +
               $"and a confidence rate of {confidence}%.";
    }

    private static string BuildBulletinContent(
        DateOnly bulletinDate,
        List<BulletinItem> items,
        int strongBuyCount,
        int buyCount,
        int sellCount,
        int strongSellCount)
    {
        var lines = new List<string>
        {
            $"# Daily Technical Analysis Bulletin - {bulletinDate:dd.MM.yyyy}",
            "",
            $"Strong Buy: {strongBuyCount} | Buy: {buyCount} | Sell: {sellCount} | Strong Sell: {strongSellCount}",
            ""
        };

        var signalOrder = new Dictionary<SignalType, int>
        {
            [SignalType.StrongBuy] = 0,
            [SignalType.Buy] = 1,
            [SignalType.Sell] = 2,
            [SignalType.StrongSell] = 3
        };

        foreach (var group in items.GroupBy(i => i.SignalType).OrderBy(g => signalOrder.GetValueOrDefault(g.Key, 99)))
        {
            lines.Add($"## {group.Key}");
            foreach (var item in group)
            {
                lines.Add($"- {item.ReasonText}");
            }
            lines.Add("");
        }

        return string.Join(Environment.NewLine, lines);
    }
}