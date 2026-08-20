using System.Diagnostics;
using System.Globalization;
using BistAdvisor.Application.Bulletins;
using BistAdvisor.Application.Indicators;
using BistAdvisor.Application.Jobs;
using BistAdvisor.Application.MarketData;
using BistAdvisor.Domain.Entities;
using BistAdvisor.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BistAdvisor.Web.Controllers;

[ApiExplorerSettings(IgnoreApi = true)]
[Authorize]
public class AdminController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IPriceDataService _priceDataService;
    private readonly ISignalService _signalService;
    private readonly IBulletinService _bulletinService;
    private readonly IMarketDataProvider _marketDataProvider;
    private readonly IJobLockService _jobLockService;

    public AdminController(
        ApplicationDbContext context,
        IPriceDataService priceDataService,
        ISignalService signalService,
        IBulletinService bulletinService,
        IMarketDataProvider marketDataProvider,
        IJobLockService jobLockService)
    {
        _context = context;
        _priceDataService = priceDataService;
        _signalService = signalService;
        _bulletinService = bulletinService;
        _marketDataProvider = marketDataProvider;
        _jobLockService = jobLockService;
    }

    public async Task<IActionResult> Index()
    {
        var stocks = await _context.Stocks
            .OrderBy(s => s.Symbol)
            .ToListAsync();

        var recentLogs = await _context.DataFetchLogs
            .Include(l => l.Stock)
            .OrderByDescending(l => l.StartedAt)
            .Take(30)
            .ToListAsync();

        var failedLogs = await _context.DataFetchLogs
            .Include(l => l.Stock)
            .Where(l => l.Status == JobStatus.Failed)
            .OrderByDescending(l => l.StartedAt)
            .Take(10)
            .ToListAsync();

        ViewData["Stocks"] = stocks;
        ViewData["RecentLogs"] = recentLogs;
        ViewData["FailedLogs"] = failedLogs;
        var settings = await _context.ApplicationSettings.OrderBy(s => s.Key).ToListAsync();
        ViewData["Settings"] = settings;

        return View();
    }

    [HttpPost]
public async Task<IActionResult> RunDataSync()
{
    var stopwatch = Stopwatch.StartNew();
    const string jobName = "DataSyncAndSignalCalculation";

    var lockAcquired = await _jobLockService.TryAcquireLockAsync(jobName);

    if (!lockAcquired)
    {
        return Json(new
        {
            success = false,
            message = "A data synchronization job is already running. Please try again shortly.",
            durationSeconds = "0.00"
        });
    }

    try
    {
        var activeStocks = await _context.Stocks.Where(s => s.IsActive).ToListAsync();

        foreach (var stock in activeStocks)
        {
            var log = new DataFetchLog
            {
                JobName = "ManualDataSync",
                StockId = stock.Id,
                StartedAt = DateTimeOffset.UtcNow,
                Status = JobStatus.Success
            };

            try
            {
                var insertedCount = await _priceDataService.SyncHistoricalDataAsync(
                    stock.Symbol, DateTimeOffset.UtcNow.AddDays(-90), DateTimeOffset.UtcNow);
                await _signalService.CalculateAndSaveSignalAsync(stock.Symbol);

                log.InsertedRowCount = insertedCount;
                log.Status = JobStatus.Success;
                log.CompletedAt = DateTimeOffset.UtcNow;
            }
            catch (Exception ex)
            {
                log.Status = JobStatus.Failed;
                log.ErrorMessage = ex.Message;
                log.CompletedAt = DateTimeOffset.UtcNow;
            }
            finally
            {
                _context.DataFetchLogs.Add(log);
            }
        }

        await _context.SaveChangesAsync();
    }
    finally
    {
        await _jobLockService.ReleaseLockAsync(jobName);
    }

    stopwatch.Stop();

    return Json(new
    {
        success = true,
        message = "Data synchronization completed.",
        durationSeconds = stopwatch.Elapsed.TotalSeconds.ToString("F2", CultureInfo.InvariantCulture)
    });
}

    [HttpPost]
    public async Task<IActionResult> GenerateBulletin()
    {
        var stopwatch = Stopwatch.StartNew();

        await _bulletinService.GenerateDailyBulletinAsync(DateOnly.FromDateTime(DateTime.UtcNow));

        stopwatch.Stop();

        return Json(new
        {
            success = true,
            message = "Bulletin generated.",
            durationSeconds = stopwatch.Elapsed.TotalSeconds.ToString("F2", CultureInfo.InvariantCulture)
        });
    }

    [HttpPost]
    public async Task<IActionResult> ToggleStock(int stockId)
    {
        var stopwatch = Stopwatch.StartNew();

        var stock = await _context.Stocks.FindAsync(stockId);
        if (stock is null)
        {
            return Json(new { success = false, message = "Stock not found." });
        }

        stock.IsActive = !stock.IsActive;
        stock.UpdatedAt = DateTimeOffset.UtcNow;
        await _context.SaveChangesAsync();

        stopwatch.Stop();

        return Json(new
        {
            success = true,
            message = stock.IsActive ? "Stock activated." : "Stock deactivated.",
            durationSeconds = stopwatch.Elapsed.TotalSeconds.ToString("F2", CultureInfo.InvariantCulture),
            isActive = stock.IsActive
        });
    }

    [HttpPost]
    public async Task<IActionResult> TestConnection()
    {
        var stopwatch = Stopwatch.StartNew();

        var isAvailable = await _marketDataProvider.IsAvailableAsync();

        stopwatch.Stop();

        return Json(new
        {
            success = isAvailable,
            message = isAvailable ? "Data source connection successful." : "Data source connection failed.",
            durationSeconds = stopwatch.Elapsed.TotalSeconds.ToString("F2", CultureInfo.InvariantCulture)
        });
    }
}