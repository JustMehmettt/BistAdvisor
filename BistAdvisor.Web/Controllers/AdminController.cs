using BistAdvisor.Application.Bulletins;
using BistAdvisor.Application.Indicators;
using BistAdvisor.Application.MarketData;
using BistAdvisor.Domain.Entities;
using BistAdvisor.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BistAdvisor.Web.Controllers;

[ApiExplorerSettings(IgnoreApi = true)]
public class AdminController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IPriceDataService _priceDataService;
    private readonly ISignalService _signalService;
    private readonly IBulletinService _bulletinService;
    private readonly IMarketDataProvider _marketDataProvider;

    public AdminController(
        ApplicationDbContext context,
        IPriceDataService priceDataService,
        ISignalService signalService,
        IBulletinService bulletinService,
        IMarketDataProvider marketDataProvider)
    {
        _context = context;
        _priceDataService = priceDataService;
        _signalService = signalService;
        _bulletinService = bulletinService;
        _marketDataProvider = marketDataProvider;   
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
        
        return View();
    }

    [HttpPost]
    [HttpPost]
    public async Task<IActionResult> RunDataSync()
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

        TempData["Message"] = "Data synchronization completed manually.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> GenerateBulletin()
    {
        await _bulletinService.GenerateDailyBulletinAsync(DateOnly.FromDateTime(DateTime.UtcNow));
        TempData["Message"] = "Bulletin generated manually.";
        return RedirectToAction(nameof(Index));   
    }

    [HttpPost]
    public async Task<IActionResult> ToggleStock(int stockId)
    {
        var stock = await _context.Stocks.FindAsync(stockId);
        if (stock is not null)
        {
            stock.IsActive = !stock.IsActive;
            stock.UpdatedAt = DateTimeOffset.UtcNow;
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> TestConnection()
    {
        var isAvailable = await _marketDataProvider.IsAvailableAsync();
        TempData["Message"] = isAvailable
            ? "Data source connection successful."
            : "Data source connection failed.";
        return RedirectToAction(nameof(Index));
    }
}