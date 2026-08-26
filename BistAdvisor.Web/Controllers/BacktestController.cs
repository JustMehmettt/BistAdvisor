using BistAdvisor.Application.Backtesting;
using BistAdvisor.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BistAdvisor.Web.Controllers;

[ApiExplorerSettings(IgnoreApi = true)]
public class BacktestController : Controller
{
    private readonly IBacktestService _backtestService;
    private readonly ApplicationDbContext _context;

    public BacktestController(IBacktestService backtestService, ApplicationDbContext context)
    {
        _backtestService = backtestService;
        _context = context;
    }

    public async Task<IActionResult> Index(string? symbol, DateOnly? fromDate, DateOnly? toDate)
    {
        var effectiveFromDate = fromDate ?? DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(-3));
        var effectiveToDate = toDate ?? DateOnly.FromDateTime(DateTime.UtcNow);

        ViewData["Symbols"] = await _context.Stocks
            .Where(s => s.IsActive)
            .OrderBy(s => s.Symbol)
            .Select(s => s.Symbol)
            .ToListAsync();

        ViewData["CurrentSymbol"] = symbol;
        ViewData["CurrentFromDate"] = effectiveFromDate;
        ViewData["CurrentToDate"] = effectiveToDate;

        if (Request.Query.Count == 0)
        {
            return View(null);
        }
        
        var result = await _backtestService.RunBacktestAsync(symbol, effectiveFromDate, effectiveToDate);
        
        return View(result);
    }
}