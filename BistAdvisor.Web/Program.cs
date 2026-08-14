using BistAdvisor.Application.Bulletins;
using BistAdvisor.Application.Dtos;
using BistAdvisor.Application.Indicators;
using BistAdvisor.Application.MarketData;
using BistAdvisor.Infrastructure.Bulletins;
using BistAdvisor.Infrastructure.Data;
using BistAdvisor.Infrastructure.MarketData;
using Microsoft.EntityFrameworkCore;
using BistAdvisor.Infrastructure.Indicators;
using DomainSignalType = BistAdvisor.Domain.Entities.SignalType;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddControllersWithViews();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IMarketDataProvider, MockMarketDataProvider>();
builder.Services.AddScoped<IPriceDataService, PriceDataService>();
builder.Services.AddScoped<ISignalService, SignalService>();
builder.Services.AddScoped<IBulletinService, BulletinService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await DataSeeder.SeedAsync(db);
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

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

app.MapGet("/api/stocks/{symbol}/signals", async (string symbol, ApplicationDbContext db, int page = 1, int pageSize = 20) =>
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

app.MapGet("/api/signals/latest", async (ApplicationDbContext db, string? signalType = null, int page = 1, int pageSize = 20) =>
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

app.MapPost("/test-bulletin/", async (IBulletinService BulletinService) =>
{
    var bulletin = await BulletinService.GenerateDailyBulletinAsync(DateOnly.FromDateTime(DateTime.UtcNow));

    return Results.Ok(new
    {
        bulletin.Id,
        bulletin.Title,
        bulletin.Summary,
        bulletin.Status,
        ItemCount = bulletin.Items.Count,
        bulletin.Content
    });
});

app.Run();