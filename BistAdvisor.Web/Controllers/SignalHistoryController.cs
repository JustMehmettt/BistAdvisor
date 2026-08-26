using BistAdvisor.Application.Dtos;
using BistAdvisor.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BistAdvisor.Web.Controllers;

[ApiExplorerSettings(IgnoreApi = true)]
public class SignalHistoryController : Controller
{
    private readonly ApplicationDbContext _context;

    public SignalHistoryController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(string? signalType, string symbol, int page = 1, int pageSize = 30)
    {
        var query = _context.SignalChanges
            .Include(c => c.Stock)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(signalType) &&
            Enum.TryParse<Domain.Entities.SignalType>(signalType, true, out var parsedType))
        {
            query = query.Where(c => c.NewSignalType == parsedType);
        }

        if (!string.IsNullOrWhiteSpace(symbol))
        {
            query = query.Where(c =>
                c.Stock.Symbol.Contains(symbol) ||
                c.Stock.CompanyName.Contains(symbol));
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

        ViewData["CurrentSignalType"] = signalType;
        ViewData["Page"] = page;
        ViewData["PageSize"] = pageSize;
        ViewData["TotalCount"] = totalCount;
        ViewData["TotalPages"] = (int)Math.Ceiling(totalCount / (double)pageSize);

        return View(changes);
    }
    
    [HttpGet]
    public async Task<IActionResult> Table(string? signalType, string? symbol, int page = 1, int pageSize = 30)
    {
        var query = _context.SignalChanges
            .Include(c => c.Stock)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(signalType) &&
            Enum.TryParse<Domain.Entities.SignalType>(signalType, true, out var parsedType))
        {
            query = query.Where(c => c.NewSignalType == parsedType);
        }

        if (!string.IsNullOrWhiteSpace(symbol))
        {
            query = query.Where(c =>
                c.Stock.Symbol.Contains(symbol) ||
                c.Stock.CompanyName.Contains(symbol));
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

        ViewData["CurrentSignalType"] = signalType;
        ViewData["CurrentSymbol"] = symbol;
        ViewData["Page"] = page;
        ViewData["PageSize"] = pageSize;
        ViewData["TotalCount"] = totalCount;
        ViewData["TotalPages"] = (int)Math.Ceiling(totalCount / (double)pageSize);

        return PartialView("_SignalHistoryTable", changes);
    }
}