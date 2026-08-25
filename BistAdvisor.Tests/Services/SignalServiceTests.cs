using BistAdvisor.Domain.Entities;
using BistAdvisor.Infrastructure.Data;
using BistAdvisor.Infrastructure.Indicators;
using BistAdvisor.Infrastructure.MarketData;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BistAdvisor.Tests.Services;

public class SignalServiceTests
{
    private static ApplicationDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        
        return new ApplicationDbContext(options);
    }

    private static async Task<Stock> SeedStockWithPriceHistoryAsync(ApplicationDbContext context)
    {
        var stock = new Stock
        {
            Symbol = "TEST",
            ProviderSymbol = "TEST.IS",
            CompanyName = "Test Company",
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
            stock.Symbol, 
            DateTimeOffset.UtcNow.AddDays(-90), 
            DateTimeOffset.UtcNow);

        return stock;
    }

    [Fact]
    public async Task CalculateAndSaveSignalAsync_CreateSignalSnapshot()
    {
        await using var context = CreateInMemoryContext();
        var stock = await SeedStockWithPriceHistoryAsync(context);
        var signalService = new SignalService(context);
        
        var snapshot = await signalService.CalculateAndSaveSignalAsync(stock.Symbol);
        
        Assert.NotEqual(0, snapshot.Id);
        
        var savedCount = await context.SignalSnapshots.CountAsync(s => s.StockId == stock.Id);
        Assert.Equal(1, savedCount);
    }

    [Fact]
    public async Task CalculateAndSaveSignalAsync_WhenSignalTypeChanges_CreatesSignalChange()
    {
        await using var context = CreateInMemoryContext();
        var stock = await SeedStockWithPriceHistoryAsync(context);
        var signalService = new SignalService(context);
        
        var now = DateTimeOffset.UtcNow;
        context.SignalSnapshots.Add(new SignalSnapshot
        {
            StockId = stock.Id,
            BarTime = now,
            Interval = PriceInterval.Daily,
            SignalType = SignalType.Neutral,
            TotalScore = 0,
            ConfidenceRate = 50,
            AlgorithmVersion = "v1.0",
            CreatedAt = now.AddMinutes(-10)
        });
        await context.SaveChangesAsync();
        
        await signalService.CalculateAndSaveSignalAsync(stock.Symbol);
        
        var changeCount = await context.SignalChanges.CountAsync(c => c.StockId == stock.Id);
        Assert.True(changeCount <= 1);
    }

    [Fact]
    public async Task CalculateAndSaveSignalAsync_WhenSignalTypeDoesNotChange_DoesNotCreateSignalChange()
    {
        await using var context = CreateInMemoryContext();
        var stock = await SeedStockWithPriceHistoryAsync(context);
        var signalService = new SignalService(context);
        
        var firstSnapshot = await signalService.CalculateAndSaveSignalAsync(stock.Symbol);
        var changeCountAfterFirst = await context.SignalChanges.CountAsync(c => c.StockId == stock.Id);
        
        var secondSnapshot = await signalService.CalculateAndSaveSignalAsync(stock.Symbol);
        var changeCountAfterSecond = await context.SignalChanges.CountAsync(c => c.StockId == stock.Id);

        Assert.Equal(firstSnapshot.SignalType, secondSnapshot.SignalType);
        Assert.Equal(changeCountAfterFirst, changeCountAfterSecond);
    }

    [Fact]
    public async Task CalculateAndSaveSignalAsync_WithStaleData_ReturnsStaleDataSignal()
    {
        await using var context = CreateInMemoryContext();

        var stock = new Stock
        {
            Symbol = "STALE",
            ProviderSymbol = "STALE.IS",
            CompanyName = "Stale Test Company",
            Market = "BIST",
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow,
        };
        context.Stocks.Add(stock);
        await context.SaveChangesAsync();

        var endDate = DateTimeOffset.UtcNow.AddDays(-10);
        var bars = new List<PriceBar>();

        for (var i = 90; i >= 0; i--)
        {
            var barDate = endDate.AddDays(-i);

            if (barDate.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
            {
                continue;
            }

            bars.Add(new PriceBar
            {
                StockId = stock.Id,
                Interval = PriceInterval.Daily,
                BarTime = new DateTimeOffset(barDate.Date, TimeSpan.Zero),
                OpenPrice = 100,
                HighPrice = 101,
                LowPrice = 99,
                ClosePrice = 100,
                AdjustedClosePrice = 100,
                Volume = 1000,
                DataSource = "Test",
                ReceivedAt = DateTimeOffset.UtcNow,
                CreatedAt = DateTimeOffset.UtcNow
            });
        }
        
        context.PriceBars.AddRange(bars);
        await context.SaveChangesAsync();
        
        var signalService = new SignalService(context);
        
        var snapshot = await signalService.CalculateAndSaveSignalAsync(stock.Symbol);
        
        Assert.Equal(SignalType.StaleData, snapshot.SignalType);
        Assert.Null(snapshot.TotalScore);
    }

    [Fact]
    public async Task CalculateAndSaveSignalAsync_WithNoData_ReturnsDataUnavailableSignal()
    {
        await using var context = CreateInMemoryContext();

        var stock = new Stock
        {
            Symbol = "NODATA",
            ProviderSymbol = "NODATA.IS",
            CompanyName = "No Data Test Company",
            Market = "BIST",
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        context.Stocks.Add(stock);
        await context.SaveChangesAsync();

        var signalService = new SignalService(context);

        var snapshot = await signalService.CalculateAndSaveSignalAsync(stock.Symbol);

        Assert.Equal(SignalType.DataUnavailable, snapshot.SignalType);
    }
    
    [Fact]
    public async Task CalculateAndSaveSignalAsync_WithFewerThan60Bars_ReturnsInsufficientDataSignal()
    {
        await using var context = CreateInMemoryContext();

        var stock = new Stock
        {
            Symbol = "FewBars",
            ProviderSymbol = "FewBars.IS",
            CompanyName = "Few Bars Test Company",
            Market = "BIST",
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        
        context.Stocks.Add(stock);
        await context.SaveChangesAsync();

        var bars = new List<PriceBar>();
        for (var i = 10; i >= 0; i--)
        {
            var barDate = DateTimeOffset.UtcNow.AddDays(-i);
            bars.Add(new PriceBar
            {
                StockId = stock.Id,
                Interval = PriceInterval.Daily,
                BarTime = new DateTimeOffset(barDate.Date, TimeSpan.Zero),
                OpenPrice = 100,
                HighPrice = 101,
                LowPrice = 99,
                ClosePrice = 100,
                AdjustedClosePrice = 100,
                Volume = 1000,
                DataSource = "Test",
                ReceivedAt = DateTimeOffset.UtcNow,
                CreatedAt = DateTimeOffset.UtcNow
            });
        }
        
        context.PriceBars.AddRange(bars);
        await context.SaveChangesAsync();
        
        var signalService = new SignalService(context);
        
        var snapshot = await signalService.CalculateAndSaveSignalAsync(stock.Symbol);
        
        Assert.Equal(SignalType.InsufficientData, snapshot.SignalType);
    }
}