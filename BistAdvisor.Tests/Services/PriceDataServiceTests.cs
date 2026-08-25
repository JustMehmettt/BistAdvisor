using BistAdvisor.Application.MarketData;
using BistAdvisor.Domain.Entities;
using BistAdvisor.Infrastructure.Data;
using BistAdvisor.Infrastructure.MarketData;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BistAdvisor.Tests.Services;

public class PriceDataServiceTests
{
    private static ApplicationDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    private static async Task<Stock> SeedStockAsync(ApplicationDbContext context, string symbol = "TEST")
    {
        var stock = new Stock
        {
            Symbol = symbol,
            ProviderSymbol = $"{symbol}.IS",
            CompanyName = "Test Company",
            Market = "BIST",
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        context.Stocks.Add(stock);
        await context.SaveChangesAsync();

        return stock;
    }

    [Fact]
    public async Task SyncHistoricalDataAsync_FirstCall_InsertsAllPoints()
    {
        await using var context = CreateInMemoryContext();
        var stock = await SeedStockAsync(context);
        var provider = new MockMarketDataProvider();
        var service = new PriceDataService(context, provider);

        var insertedCount = await service.SyncHistoricalDataAsync(
            stock.Symbol,
            DateTimeOffset.UtcNow.AddDays(-30),
            DateTimeOffset.UtcNow);

        Assert.True(insertedCount > 0);

        var savedCount = await context.PriceBars.CountAsync(p => p.StockId == stock.Id);
        Assert.Equal(insertedCount, savedCount);
    }

    [Fact]
    public async Task SyncHistoricalDataAsync_SecondCallWithSameRange_InsertsZeroNewRecords()
    {
        await using var context = CreateInMemoryContext();
        var stock = await SeedStockAsync(context);
        var provider = new MockMarketDataProvider();
        var service = new PriceDataService(context, provider);

        var from = DateTimeOffset.UtcNow.AddDays(-30);
        var to = DateTimeOffset.UtcNow;

        var firstInsertedCount = await service.SyncHistoricalDataAsync(
            stock.Symbol,
            from,
            to);

        var secondInsertedCount = await service.SyncHistoricalDataAsync(
            stock.Symbol,
            from,
            to);

        Assert.True(firstInsertedCount > 0);
        Assert.Equal(0, secondInsertedCount);
    }

    [Fact]
    public async Task SyncHistoricalDataAsync_WithUnknownSymbol_ThrowsInvalidOperationException()
    {
        await using var context = CreateInMemoryContext();
        var provider = new MockMarketDataProvider();
        var service = new PriceDataService(context, provider);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.SyncHistoricalDataAsync(
                "UNKNOWN",
                DateTimeOffset.UtcNow.AddDays(-30),
                DateTimeOffset.UtcNow));
    }

    [Fact]
    public async Task SyncHistoricalDataAsync_WithTransientFailures_RetriesAndEventuallySucceeds()
    {
        await using var context = CreateInMemoryContext();
        var stock = await SeedStockAsync(context);
        var provider = new FailingMarketDataProvider(failuresBeforeSuccess: 2);
        var service = new PriceDataService(context, provider);

        var insertedCount = await service.SyncHistoricalDataAsync(
            stock.Symbol,
            DateTimeOffset.UtcNow.AddDays(-30),
            DateTimeOffset.UtcNow);

        Assert.Equal(3, provider.AttemptCount);
        Assert.True(insertedCount > 0);
    }

    [Fact]
    public async Task SyncHistoricalDataAsync_WithPersistentFailures_ThrowsAfterMaxRetries()
    {
        await using var context = CreateInMemoryContext();
        var stock = await SeedStockAsync(context);
        var provider = new FailingMarketDataProvider(failuresBeforeSuccess: 10);
        var service = new PriceDataService(context, provider);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.SyncHistoricalDataAsync(
                stock.Symbol,
                DateTimeOffset.UtcNow.AddDays(-30),
                DateTimeOffset.UtcNow));

        Assert.Equal(3, provider.AttemptCount);
    }

    [Fact]
    public async Task SyncHistoricalDataAsync_WithPersistentFailures_LogsFailedRawLog()
    {
        await using var context = CreateInMemoryContext();
        var stock = await SeedStockAsync(context);
        var provider = new FailingMarketDataProvider(failuresBeforeSuccess: 10);
        var service = new PriceDataService(context, provider);

        try
        {
            await service.SyncHistoricalDataAsync(
                stock.Symbol,
                DateTimeOffset.UtcNow.AddDays(-30),
                DateTimeOffset.UtcNow);
        }
        catch (InvalidOperationException)
        {
        }

        var rawLog = await context.MarketDataRawLogs
            .Where(l => l.StockId == stock.Id)
            .OrderByDescending(l => l.FetchedAt)
            .FirstOrDefaultAsync();

        Assert.NotNull(rawLog);
        Assert.False(rawLog!.WasSuccessful);
        Assert.Equal(3, rawLog.RetryCount);
    }

    [Fact]
    public async Task ProcessingMultipleStocks_WhenOneFails_OthersStillSucceed()
    {
        await using var context = CreateInMemoryContext();

        var goodStock1 = await SeedStockAsync(context, symbol: "GOOD1");
        var badStock = await SeedStockAsync(context, symbol: "BAD1");
        var goodStock2 = await SeedStockAsync(context, symbol: "GOOD2");

        var goodProvider = new MockMarketDataProvider();
        var badProvider = new FailingMarketDataProvider(failuresBeforeSuccess: 100);

        var goodService = new PriceDataService(context, goodProvider);
        var badService = new PriceDataService(context, badProvider);

        var results = new List<(string Symbol, bool Success)>();

        foreach (var (stock, service) in new[]
                 {
                     (goodStock1, goodService),
                     (badStock, badService),
                     (goodStock2, goodService)
                 })
        {
            try
            {
                await service.SyncHistoricalDataAsync(
                    stock.Symbol, DateTimeOffset.UtcNow.AddDays(-30), DateTimeOffset.UtcNow);
                results.Add((stock.Symbol, true));
            }
            catch (InvalidOperationException)
            {
                results.Add((stock.Symbol, false));
            }
        }

        Assert.True(results.First(r => r.Symbol == "GOOD1").Success);
        Assert.False(results.First(r => r.Symbol == "BAD1").Success);
        Assert.True(results.First(r => r.Symbol == "GOOD2").Success);

        var good1Bars = await context.PriceBars.CountAsync(p => p.StockId == goodStock1.Id);
        var good2Bars = await context.PriceBars.CountAsync(p => p.StockId == goodStock2.Id);

        Assert.True(good1Bars > 0);
        Assert.True(good2Bars > 0);
    }
}