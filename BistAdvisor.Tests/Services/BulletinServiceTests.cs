using BistAdvisor.Domain.Entities;
using BistAdvisor.Infrastructure.Bulletins;
using BistAdvisor.Infrastructure.Data;
using BistAdvisor.Infrastructure.MarketData;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BistAdvisor.Tests.Services;

public class BulletinServiceTests
{
    private static ApplicationDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    private static async Task<Stock> SeedStockWithSignalAsync(
        ApplicationDbContext context, string symbol, SignalType signalType, decimal? score)
    {
        var stock = new Stock
        {
            Symbol = symbol,
            ProviderSymbol = $"{symbol}.IS",
            CompanyName = $"{symbol} Company",
            Market = "BIST",
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        context.Stocks.Add(stock);
        await context.SaveChangesAsync();

        var provider = new MockMarketDataProvider();
        var priceDataService = new PriceDataService(context, provider);
        await priceDataService.SyncHistoricalDataAsync(
            symbol, DateTimeOffset.UtcNow.AddDays(-90), DateTimeOffset.UtcNow);

        var now = DateTimeOffset.UtcNow;
        context.SignalSnapshots.Add(new SignalSnapshot
        {
            StockId = stock.Id,
            BarTime = now,
            Interval = PriceInterval.Daily,
            SignalType = signalType,
            TotalScore = score,
            ConfidenceRate = 70,
            AlgorithmVersion = "v1.0",
            CreatedAt = now
        });
        await context.SaveChangesAsync();

        return stock;
    }

    [Fact]
    public async Task GenerateDailyBulletinAsync_CalledTwiceOnSameDay_MarksFirstAsRevised()
    {
        await using var context = CreateInMemoryContext();
        var service = new BulletinService(context);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var firstBulletin = await service.GenerateDailyBulletinAsync(today);
        var secondBulletin = await service.GenerateDailyBulletinAsync(today);

        var refreshedFirst = await context.DailyBulletins.FindAsync(firstBulletin.Id);

        Assert.Equal(BulletinStatus.Revised, refreshedFirst!.Status);
        Assert.Equal(BulletinStatus.Active, secondBulletin.Status);
        Assert.NotEqual(firstBulletin.Id, secondBulletin.Id);
    }

    [Fact]
    public async Task GenerateDailyBulletinAsync_CalledTwiceOnSameDay_OnlyOneActiveBulletinExists()
    {
        await using var context = CreateInMemoryContext();
        var service = new BulletinService(context);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        await service.GenerateDailyBulletinAsync(today);
        await service.GenerateDailyBulletinAsync(today);
        await service.GenerateDailyBulletinAsync(today);

        var activeBulletinsToday = await context.DailyBulletins
            .Where(b => b.BulletinDate == today && b.Status == BulletinStatus.Active)
            .CountAsync();

        Assert.Equal(1, activeBulletinsToday);
    }

    [Fact]
    public async Task GenerateDailyBulletinAsync_OnlyIncludesStocksWithSignalChangeToday()
    {
        await using var context = CreateInMemoryContext();

        var changedStock = await SeedStockWithSignalAsync(context, "CHANGED", SignalType.Buy, 25m);
        var unchangedStock = await SeedStockWithSignalAsync(context, "SAME", SignalType.Buy, 30m);

        var now = DateTimeOffset.UtcNow;

        context.SignalChanges.Add(new SignalChange
        {
            StockId = changedStock.Id,
            PreviousSignalType = SignalType.Neutral,
            NewSignalType = SignalType.Buy,
            PreviousScore = 0,
            NewScore = 25,
            ChangeTime = now,
            ChangeReason = "Test setup",
            AlgorithmVersion = "v1.0",
            CreatedAt = now
        });
        await context.SaveChangesAsync();

        var service = new BulletinService(context);
        var bulletin = await service.GenerateDailyBulletinAsync(DateOnly.FromDateTime(DateTime.UtcNow));

        var includedSymbols = bulletin.Items.Select(i => i.Stock.Symbol).ToList();

        Assert.Contains("CHANGED", includedSymbols);
        Assert.DoesNotContain("SAME", includedSymbols);
    }
}