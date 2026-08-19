using BistAdvisor.Application.Dtos;
using BistAdvisor.Domain.Entities;
using BistAdvisor.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BistAdvisor.Web.Controllers;

public class BulletinController : Controller
{
    private readonly ApplicationDbContext _context;
    
    public BulletinController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(DateOnly? date)
    {
        var query = _context.DailyBulletins
            .Include(b => b.Items)
            .ThenInclude(i => i.Stock)
            .AsQueryable();

        DailyBulletin? bulletin;

        if (date.HasValue)
        {
            bulletin = await query
                .Where(b => b.BulletinDate == date.Value)
                .OrderByDescending(b => b.GeneratedAt)
                .FirstOrDefaultAsync();
        }

        else
        {
            bulletin = await query
                .Where(b => b.Status == BulletinStatus.Active)
                .OrderByDescending(b => b.GeneratedAt)
                .FirstOrDefaultAsync();
        }

        var availableDates = await _context.DailyBulletins
            .Select(b => b.BulletinDate)
            .Distinct()
            .OrderByDescending(d => d)
            .Take(30)
            .ToListAsync();
        
        ViewData["AvailableDates"] = availableDates;
        ViewData["SelectedDate"] = date;

        if (bulletin is null)
        {
            return View((BulletinDto?)null);
        }

        var dto = new BulletinDto
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

        return View(dto);
    }
}