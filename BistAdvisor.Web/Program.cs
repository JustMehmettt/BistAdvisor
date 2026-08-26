using System.Runtime.InteropServices.JavaScript;
using BistAdvisor.Application.Bulletins;
using BistAdvisor.Application.Dtos;
using BistAdvisor.Application.Indicators;
using BistAdvisor.Application.Jobs;
using BistAdvisor.Application.MarketData;
using BistAdvisor.Domain.Entities;
using BistAdvisor.Infrastructure.Bulletins;
using BistAdvisor.Infrastructure.Data;
using BistAdvisor.Infrastructure.MarketData;
using Microsoft.EntityFrameworkCore;
using BistAdvisor.Infrastructure.Indicators;
using BistAdvisor.Infrastructure.Jobs;
using DomainSignalType = BistAdvisor.Domain.Entities.SignalType;
using Microsoft.AspNetCore.Mvc;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft.AspNetCore", Serilog.Events.LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", Serilog.Events.LogEventLevel.Warning)
    .WriteTo.Console()
    .WriteTo.File("logs/bistadvisor-web-.log", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 14)
    .CreateLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);
    builder.Host.UseSerilog();

    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddControllersWithViews();

    builder.Services.AddAuthentication("BistAdvisorAuth")
        .AddCookie("BistAdvisorAuth", options =>
        {
            options.LoginPath = "/Account/Login";
            options.AccessDeniedPath = "/Account/Login";
            options.ExpireTimeSpan = TimeSpan.FromHours(8);
        });

    builder.Services.AddAuthorization();
    builder.Services.AddSwaggerGen();

    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

    builder.Services.AddScoped<IMarketDataProvider, YahooMarketDataProvider>();
    builder.Services.AddScoped<IPriceDataService, PriceDataService>();
    builder.Services.AddScoped<ISignalService, SignalService>();
    builder.Services.AddScoped<IBulletinService, BulletinService>();
    builder.Services.AddScoped<IJobLockService, JobLockService>();
    builder.Services.AddScoped<IMarketHoursService, MarketHoursService>();
    
    var app = builder.Build();

    if (!app.Environment.IsEnvironment("Testing"))
    {
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await DataSeeder.SeedAsync(dbContext);
    }

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();
    app.UseStaticFiles();
    app.UseRouting();
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Dashboard}/{action=Index}/{id?}");

    app.MapGet("/api/stocks", async (ApplicationDbContext db, int page = 1, int pageSize = 20, string? sector = null) =>
    {
        var query = db.Stocks.Where(s => s.IsActive).AsQueryable();

        if (!string.IsNullOrWhiteSpace(sector))
        {
            query = query.Where(s => s.Sector == sector);
        }

        var totalCount = await query.CountAsync();

        var stocks = await query
            .OrderBy(s => s.Symbol)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(s => new StockDto
            {
                Id = s.Id,
                Symbol = s.Symbol,
                CompanyName = s.CompanyName,
                Sector = s.Sector,
                IsActive = s.IsActive
            })
            .ToListAsync();

        return Results.Ok(new PagedResult<StockDto>
        {
            Items = stocks,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        });
    });

    app.MapGet("/api/stocks/{symbol}", async (string symbol, ApplicationDbContext db) =>
    {
        var stock = await db.Stocks.FirstOrDefaultAsync(s => s.Symbol == symbol);

        if (stock is null)
        {
            return Results.NotFound(new { Message = $"Stock '{symbol}' not found." });
        }

        return Results.Ok(new StockDto
        {
            Id = stock.Id,
            Symbol = stock.Symbol,
            CompanyName = stock.CompanyName,
            Sector = stock.Sector,
            IsActive = stock.IsActive
        });
    });

    app.MapGet("/api/stocks/{symbol}/signals",
        async (string symbol, ApplicationDbContext db, int page = 1, int pageSize = 20) =>
        {
            var stock = await db.Stocks.FirstOrDefaultAsync(s => s.Symbol == symbol);

            if (stock is null)
            {
                return Results.NotFound(new { Message = $"Stock '{symbol}' not found." });
            }

            var query = db.SignalSnapshots.Where(s => s.StockId == stock.Id);
            var totalCount = await query.CountAsync();

            var signals = await query
                .OrderByDescending(s => s.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(s => new SignalDto
                {
                    Symbol = stock.Symbol,
                    CompanyName = stock.CompanyName,
                    SignalType = s.SignalType.ToString(),
                    TotalScore = s.TotalScore,
                    ConfidenceRate = s.ConfidenceRate,
                    Explanation = s.Explanation,
                    CreatedAt = s.CreatedAt
                })
                .ToListAsync();

            return Results.Ok(new PagedResult<SignalDto>
            {
                Items = signals,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount
            });
        });

    app.MapGet("/api/signals/latest",
        async (ApplicationDbContext db, string? signalType = null, int page = 1, int pageSize = 20) =>
        {
            var latestTimestamps = db.SignalSnapshots
                .GroupBy(s => s.StockId)
                .Select(g => new { StockId = g.Key, MaxCreatedAt = g.Max(s => s.CreatedAt) });

            var latestSignals = db.SignalSnapshots
                .Join(latestTimestamps,
                    s => new { s.StockId, s.CreatedAt },
                    t => new { t.StockId, CreatedAt = t.MaxCreatedAt },
                    (s, t) => s);

            if (!string.IsNullOrWhiteSpace(signalType) &&
                Enum.TryParse<DomainSignalType>(signalType, true, out var parsedType))
            {
                latestSignals = latestSignals.Where(s => s.SignalType == parsedType);
            }

            var joined = latestSignals
                .Join(db.Stocks, s => s.StockId, st => st.Id, (s, st) => new { Signal = s, Stock = st });

            var totalCount = await joined.CountAsync();

            var results = await joined
                .OrderByDescending(x => x.Signal.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new SignalDto
                {
                    Symbol = x.Stock.Symbol,
                    CompanyName = x.Stock.CompanyName,
                    SignalType = x.Signal.SignalType.ToString(),
                    TotalScore = x.Signal.TotalScore,
                    ConfidenceRate = x.Signal.ConfidenceRate,
                    Explanation = x.Signal.Explanation,
                    CreatedAt = x.Signal.CreatedAt
                })
                .ToListAsync();

            return Results.Ok(new PagedResult<SignalDto>
            {
                Items = results,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount
            });
        });

    app.MapGet("/api/stocks/{symbol}/prices",
        async (string symbol, ApplicationDbContext db, int page = 1, int pageSize = 30) =>
        {
            var stock = await db.Stocks.FirstOrDefaultAsync(s => s.Symbol == symbol);
            if (stock is null)
            {
                return Results.NotFound(new { Message = $"Stock '{symbol}' not found." });
            }

            var query = db.PriceBars
                .Where(p => p.StockId == stock.Id && p.Interval == PriceInterval.Daily)
                .OrderByDescending(p => p.BarTime);

            var totalCount = await query.CountAsync();

            var prices = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new PriceDto
                {
                    BarTime = p.BarTime,
                    Open = p.OpenPrice,
                    High = p.HighPrice,
                    Low = p.LowPrice,
                    Close = p.ClosePrice,
                    Volume = p.Volume
                })
                .ToListAsync();

            return Results.Ok(new PagedResult<PriceDto>
            {
                Items = prices,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount
            });
        });

    app.MapGet("/api/stocks/{symbol}/indicators",
        async (string symbol, ApplicationDbContext db, int page = 1, int pageSize = 30) =>
        {
            var stock = await db.Stocks.FirstOrDefaultAsync(s => s.Symbol == symbol);
            if (stock is null)
            {
                return Results.NotFound(new { Message = $"Stock '{symbol}' not found." });
            }

            var query = db.IndicatorResults
                .Where(r => r.StockId == stock.Id && r.Interval == PriceInterval.Daily)
                .OrderByDescending(r => r.BarTime);

            var totalCount = await query.CountAsync();

            var indicators = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(r => new IndicatorDto
                {
                    BarTime = r.BarTime,
                    RsiValue = r.RsiValue,
                    MacdValue = r.MacdValue,
                    MacdSignalValue = r.MacdSignalValue,
                    MacdHistogramValue = r.MacdHistogramValue,
                    Ema20 = r.Ema20,
                    Ema50 = r.Ema50,
                    BollingerUpper = r.BollingerUpper,
                    BollingerMiddle = r.BollingerMiddle,
                    BollingerLower = r.BollingerLower,
                    StochasticK = r.StochasticK,
                    StochasticD = r.StochasticD
                })
                .ToListAsync();

            return Results.Ok(new PagedResult<IndicatorDto>
            {
                Items = indicators,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount
            });
        });

    app.MapGet("api/signals/changes",
        async (ApplicationDbContext db, string? symbol = null, int page = 1, int pageSize = 30) =>
        {
            var query = db.SignalChanges.Include(c => c.Stock).AsQueryable();

            if (!string.IsNullOrWhiteSpace(symbol))
            {
                query = query.Where(c => c.Stock.Symbol == symbol);
            }

            var totalCount = await query.CountAsync();

            var changes = await query
                .OrderByDescending(c => c.ChangeTime)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(c => new SignalChangeDto
                {
                    Symbol = c.Stock.Symbol,
                    CompanyName = c.Stock.CompanyName,
                    PreviousSignalType = c.PreviousSignalType.ToString(),
                    NewSignalType = c.NewSignalType.ToString(),
                    PreviousScore = c.PreviousScore,
                    NewScore = c.NewScore,
                    ChangeTime = c.ChangeTime,
                    ChangeReason = c.ChangeReason
                })
                .ToListAsync();

            return Results.Ok(new PagedResult<SignalChangeDto>
            {
                Items = changes,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount
            });
        });

    app.MapGet("/api/bulletins/today", async (ApplicationDbContext db) =>
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var bulletin = await db.DailyBulletins
            .Include(b => b.Items)
            .ThenInclude(i => i.Stock)
            .Where(b => b.BulletinDate == today && b.Status == BulletinStatus.Active)
            .OrderByDescending(b => b.GeneratedAt)
            .FirstOrDefaultAsync();

        if (bulletin is null)
        {
            return Results.NotFound(new { Message = "No active bulletin found for today." });
        }

        return Results.Ok(MapBulletin(bulletin));
    });

    app.MapGet("/api/bulletins/{date}", async (DateOnly date, ApplicationDbContext db) =>
    {
        var bulletin = await db.DailyBulletins
            .Include(b => b.Items)
            .ThenInclude(i => i.Stock)
            .Where(b => b.BulletinDate == date)
            .OrderByDescending(b => b.GeneratedAt)
            .FirstOrDefaultAsync();

        if (bulletin is null)
        {
            return Results.NotFound(new { Message = $"No bulletin found for {date:yyyy-MM-dd}." });
        }

        return Results.Ok(MapBulletin(bulletin));
    });

    app.MapGet("/api/jobs", async (ApplicationDbContext db, int page = 1, int pageSize = 30) =>
    {
        var query = db.DataFetchLogs.Include(l => l.Stock).OrderByDescending(l => l.StartedAt);

        var totalCount = await query.CountAsync();

        var jobs = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(l => new JobStatusDto
            {
                Id = l.Id,
                JobName = l.JobName,
                StockSymbol = l.Stock != null ? l.Stock.Symbol : null,
                StartedAt = l.StartedAt,
                CompletedAt = l.CompletedAt,
                Status = l.Status.ToString(),
                InsertedRowCount = l.InsertedRowCount,
                ErrorMessage = l.ErrorMessage
            })
            .ToListAsync();

        return Results.Ok(new PagedResult<JobStatusDto>
        {
            Items = jobs,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        });
    });

    app.MapPost("/api/post/data-sync", async (
        ApplicationDbContext db, IPriceDataService priceDataService, ISignalService signalService,
        IJobLockService jobLockService) =>
    {
        const string jobName = "DataSyncAndSignalCalculation";
        var lockAcquired = await jobLockService.TryAcquireLockAsync(jobName);

        if (!lockAcquired)
        {
            return Results.Conflict(new { Message = "A data synchronization job is already running." });
        }

        try
        {
            var activeStocks = await db.Stocks.Where(s => s.IsActive).ToListAsync();
            var processedCount = 0;

            foreach (var stock in activeStocks)
            {
                try
                {
                    await priceDataService.SyncHistoricalDataAsync(
                        stock.Symbol, DateTimeOffset.UtcNow.AddDays(-90), DateTimeOffset.UtcNow);
                    await signalService.CalculateAndSaveSignalAsync(stock.Symbol);
                    processedCount++;
                }
                catch
                {
                    
                }
            }

            return Results.Ok(new
            {
                Message = "Data synchronization completed.", ProcessedCount = processedCount,
                TotalCount = activeStocks.Count
            });
        }
        finally
        {
            await jobLockService.ReleaseLockAsync(jobName);
        }
    });

    app.MapPost("/api/jobs/generate-bulletin", async (IBulletinService bulletinService) =>
    {
        var bulletin = await bulletinService.GenerateDailyBulletinAsync(DateOnly.FromDateTime(DateTime.UtcNow));

        return Results.Ok(new { bulletin.Id, bulletin.Title, bulletin.Status, ItemCount = bulletin.Items.Count });
    });

    app.MapGet("/api/system/data-health", async (ApplicationDbContext db, IMarketDataProvider marketDataProvider) =>
    {
        var isAvailable = await marketDataProvider.IsAvailableAsync();

        var activeStocks = await db.Stocks.Where(s => s.IsActive).ToListAsync();
        var totalActiveStocks = activeStocks.Count;

        var allSignals = await db.SignalSnapshots.ToListAsync();
        var latestSignals = allSignals
            .GroupBy(s => s.StockId)
            .Select(g => g.OrderByDescending(s => s.CreatedAt).First())
            .ToList();

        var now = DateTimeOffset.UtcNow;
        var stocksWithRecentData = latestSignals.Count(s => (now - s.CreatedAt) < TimeSpan.FromDays(1));
        var stocksWithStaleData = latestSignals.Count(s => (now - s.CreatedAt) >= TimeSpan.FromDays(1));

        var lastSuccessfulSync = await db.DataFetchLogs
            .Where(l => l.Status == JobStatus.Success)
            .OrderByDescending(l => l.StartedAt)
            .Select(l => l.CompletedAt)
            .FirstOrDefaultAsync();

        var failedJobsLast24Hours = await db.DataFetchLogs
            .Where(l => l.Status == JobStatus.Failed && l.StartedAt >= now.AddHours(-24))
            .CountAsync();

        return Results.Ok(new DataHealthDto
        {
            IsDataSourceAvailable = isAvailable,
            TotalActiveStocks = totalActiveStocks,
            StocksWithRecentData = stocksWithRecentData,
            StocksWithStaleData = stocksWithStaleData,
            LastSuccessfulSync = lastSuccessfulSync,
            FailedJobsLast24Hours = failedJobsLast24Hours
        });
    });


    static BulletinDto MapBulletin(DailyBulletin bulletin)
    {
        return new BulletinDto
        {
            Id = bulletin.Id,
            BulletinDate = bulletin.BulletinDate,
            Title = bulletin.Title,
            Summary = bulletin.Summary,
            Status = bulletin.Status.ToString(),
            GeneratedAt = bulletin.GeneratedAt,
            Items = bulletin.Items
                .OrderBy(i => i.Rank)
                .Select(i => new BulletinItemDto
                {
                    Symbol = i.Stock.Symbol,
                    CompanyName = i.Stock.CompanyName,
                    SignalType = i.SignalType.ToString(),
                    TotalScore = i.TotalScore,
                    ConfidenceRate = i.ConfidenceRate,
                    LastPrice = i.LastPrice,
                    DailyChangeRate = i.DailyChangeRate,
                    ReasonText = i.ReasonText
                })
                .ToList()
        };
    }

    app.Run();
    
}
catch (Exception ex) when(ex is not Microsoft.Extensions.Hosting.HostAbortedException)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
public partial class Program { }
