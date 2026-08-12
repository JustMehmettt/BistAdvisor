using BistAdvisor.Application.Dtos;
using BistAdvisor.Domain.Entities;
using BistAdvisor.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BistAdvisor.Web.Controllers;

public class StocksController : Controller
{
    private readonly ApplicationDbContext _context;

    public StocksController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(string? sector, string? signalType, decimal? minScore, string? search)
    {
        var stocks = await _context.Stocks
            .Where(s => s.IsActive)
            .ToListAsync();

        var allSignals = await _context.SignalSnapshots.ToListAsync();
        var allPrices = await _context.PriceBars
            .Where(p => p.Interval == PriceInterval.Daily)
            .ToListAsync();

        var latestSignals = allSignals
            .GroupBy(s => s.StockId)
            .Select(g => g.OrderByDescending(s => s.CreatedAt).First())
            .ToDictionary(s => s.StockId);

        var latestPrices = allPrices
            .GroupBy(p => p.StockId)
            .Select(g => g.OrderByDescending(p => p.BarTime).First())
            .ToDictionary(p => p.StockId);

        var combined = stocks.Select(s => new
        {
            Stock = s,
            Signal = latestSignals.GetValueOrDefault(s.Id),
            Price = latestPrices.GetValueOrDefault(s.Id)
        }).AsEnumerable();

        if (!string.IsNullOrWhiteSpace(sector))
        {
            combined = combined.Where(x => x.Stock.Sector == sector);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            combined = combined.Where(x =>
                x.Stock.Symbol.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                x.Stock.CompanyName.Contains(search, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(signalType) &&
            Enum.TryParse<SignalType>(signalType, true, out var parsedType))
        {
            combined = combined.Where(x => x.Signal != null && x.Signal.SignalType == parsedType);
        }

        if (minScore.HasValue)
        {
            combined = combined.Where(x => x.Signal != null && x.Signal.TotalScore >= minScore.Value);
        }

        var results = combined
            .OrderBy(x => x.Stock.Symbol)
            .Select(x => new StockListItemDto
            {
                Symbol = x.Stock.Symbol,
                CompanyName = x.Stock.CompanyName,
                Sector = x.Stock.Sector,
                LastPrice = x.Price?.ClosePrice,
                SignalType = x.Signal?.SignalType.ToString() ?? "InsufficientData",
                TotalScore = x.Signal?.TotalScore,
                ConfidenceRate = x.Signal?.ConfidenceRate,
                LastUpdate = x.Signal?.CreatedAt
            })
            .ToList();

        ViewData["Sectors"] = await _context.Stocks
            .Where(s => s.IsActive && s.Sector != null)
            .Select(s => s.Sector)
            .Distinct()
            .OrderBy(s => s)
            .ToListAsync();

        ViewData["CurrentSector"] = sector;
        ViewData["CurrentSignalType"] = signalType;
        ViewData["CurrentMinScore"] = minScore;
        ViewData["CurrentSearch"] = search;

        return View(results);
    }
}