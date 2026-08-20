using BistAdvisor.Domain.Entities;
using BistAdvisor.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BistAdvisor.Web.Controllers;

public class DashboardController : Controller
{
    private readonly ApplicationDbContext _context;

    public DashboardController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var totalStocks = await _context.Stocks.CountAsync(s => s.IsActive);

        var allSignals = await _context.SignalSnapshots.ToListAsync();
        var latestSignals = allSignals
            .GroupBy(s => s.StockId)
            .Select(g => g.OrderByDescending(s => s.CreatedAt).First())
            .ToList();

        var stocksById = await _context.Stocks.ToDictionaryAsync(s => s.Id);

        ViewData["TotalStocks"] = totalStocks;
        ViewData["StrongBuyCount"] = latestSignals.Count(s => s.SignalType == SignalType.StrongBuy);
        ViewData["BuyCount"] = latestSignals.Count(s => s.SignalType == SignalType.Buy);
        ViewData["NeutralCount"] = latestSignals.Count(s => s.SignalType == SignalType.Neutral);
        ViewData["SellCount"] = latestSignals.Count(s => s.SignalType == SignalType.Sell);
        ViewData["StrongSellCount"] = latestSignals.Count(s => s.SignalType == SignalType.StrongSell);
        ViewData["LastUpdate"] = latestSignals.Count > 0 ? latestSignals.Max(s => s.CreatedAt) : (DateTimeOffset?)null;

        var lastLog = await _context.DataFetchLogs
            .OrderByDescending(l => l.StartedAt)
            .FirstOrDefaultAsync();

        ViewData["LastJobStatus"] = lastLog?.Status;
        ViewData["LastJobTime"] = lastLog?.StartedAt;

        var topFive = latestSignals
            .Where(s => s.TotalScore.HasValue && stocksById.ContainsKey(s.StockId))
            .OrderByDescending(s => s.TotalScore)
            .Take(5)
            .Select(s => new
            {
                Symbol = stocksById[s.StockId].Symbol,
                CompanyName = stocksById[s.StockId].CompanyName,
                Score = s.TotalScore,
                SignalType = s.SignalType.ToString()
            })
            .ToList();

        var bottomFive = latestSignals
            .Where(s => s.TotalScore.HasValue && stocksById.ContainsKey(s.StockId))
            .OrderBy(s => s.TotalScore)
            .Take(5)
            .Select(s => new
            {
                Symbol = stocksById[s.StockId].Symbol,
                CompanyName = stocksById[s.StockId].CompanyName,
                Score = s.TotalScore,
                SignalType = s.SignalType.ToString()
            })
            .ToList();

        ViewData["TopFive"] = topFive;
        ViewData["BottomFive"] = bottomFive;

        var recentChanges = await _context.SignalChanges
            .Include(c => c.Stock)
            .OrderByDescending(c => c.ChangeTime)
            .Take(5)
            .ToListAsync();

        ViewData["RecentChanges"] = recentChanges;

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var todaysBulletin = await _context.DailyBulletins
            .Where(b => b.BulletinDate == today && b.Status == BulletinStatus.Active)
            .OrderByDescending(b => b.GeneratedAt)
            .FirstOrDefaultAsync();

        ViewData["HasTodaysBulletin"] = todaysBulletin is not null;

        return View();
    }
}