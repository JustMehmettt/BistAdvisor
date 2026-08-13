using BistAdvisor.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BistAdvisor.Web.Controllers;

[ApiExplorerSettings(IgnoreApi = true)]
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

        var latestTimestamps = _context.SignalSnapshots
            .GroupBy(s => s.StockId)
            .Select(g => new { StockId = g.Key, MaxCreatedAt = g.Max(s => s.CreatedAt) });

        var latestSignals = await _context.SignalSnapshots
            .Join(latestTimestamps,
                s => new { s.StockId, s.CreatedAt },
                t => new { t.StockId, CreatedAt = t.MaxCreatedAt },
                (s, t) => s)
            .ToListAsync();

        ViewData["TotalStocks"] = totalStocks;
        ViewData["StrongBuyCount"] = latestSignals.Count(s => s.SignalType == Domain.Entities.SignalType.StrongBuy);
        ViewData["BuyCount"] = latestSignals.Count(s => s.SignalType == Domain.Entities.SignalType.Buy);
        ViewData["NeutralCount"] = latestSignals.Count(s => s.SignalType == Domain.Entities.SignalType.Neutral);
        ViewData["SellCount"] = latestSignals.Count(s => s.SignalType == Domain.Entities.SignalType.Sell);
        ViewData["StrongSellCount"] = latestSignals.Count(s => s.SignalType == Domain.Entities.SignalType.StrongSell);
        ViewData["LastUpdate"] = latestSignals.Count > 0 ? latestSignals.Max(s => s.CreatedAt) : (DateTimeOffset?)null;

        return View();
    }
}