using System.Linq.Expressions;
using BistAdvisor.Application.Indicators;
using BistAdvisor.Application.MarketData;
using BistAdvisor.Infrastructure.Data;
using BistAdvisor.Infrastructure.MarketData;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using BistAdvisor.Domain.Entities;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IMarketDataProvider, MockMarketDataProvider>();
builder.Services.AddScoped<IPriceDataService, PriceDataService>();

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

app.MapGet("/test-yahoo", async (IMarketDataProvider provider) =>
{
    var isAvailable = await provider.IsAvailableAsync();
    if (!isAvailable)
    {
        return Results.Problem("Yahoo Finance şu anda erişilebilir değil.");
    }

    var data = await provider.GetHistoricalDataAsync(
        "THYAO.IS",
        DateTimeOffset.UtcNow.AddDays(-10),
        DateTimeOffset.UtcNow);

    return Results.Ok(data);
});

app.MapGet("test-rsi/{symbol}", async (string symbol, ApplicationDbContext db) =>
{
    var stock = await db.Stocks.FirstOrDefaultAsync(s => s.Symbol == symbol);
    if (stock == null)
    {
        return Results.NotFound($"'{symbol}' stock not found.");
    }

    var priceBars = await db.PriceBars
        .Where(p => p.StockId == stock.Id && p.Interval == PriceInterval.Daily)
        .OrderBy(p => p.BarTime)
        .ToListAsync();

    var rsiCalculator = new RsiCalculator();
    var rsi = rsiCalculator.Calculate(priceBars);

    return Results.Ok(new { Symbol = symbol, BarCount = priceBars.Count, Rsi = rsi });
});

app.MapGet("/test-macd/{symbol}", async (string symbol, ApplicationDbContext db) =>
{
    var stock = await db.Stocks.FirstOrDefaultAsync(s => s.Symbol == symbol);
    if (stock is null)
    {
        return Results.NotFound($"'{symbol}' stock not found.");
    }
    
    var priceBars = await db.PriceBars
        .Where(p => p.StockId == stock.Id && p.Interval == PriceInterval.Daily)
        .OrderBy(p => p.BarTime)
        .ToListAsync();
    
    var macdCalculator = new MacdCalculator();
    var macd = macdCalculator.Calculate(priceBars);
    
    return Results.Ok(new
    {
        Symbol = symbol, 
        BarCount = priceBars.Count,
        macd.MacdLine,
        macd.SignalLine,
        macd.Histogram
    });
});

app.MapGet("/test-ema/{symbol}", async (string symbol, ApplicationDbContext db) =>
{
    var stock = await db.Stocks.FirstOrDefaultAsync(s => s.Symbol == symbol);
    if (stock is null)
    {
        return Results.NotFound($"'{symbol}' stock not found.");
    }

    var priceBars = await db.PriceBars
        .Where(p => p.StockId == stock.Id && p.Interval == PriceInterval.Daily)
        .OrderBy(p => p.BarTime)
        .ToListAsync();

    var emaCalculator = new EmaTrendCalculator();
    var emaTrend = emaCalculator.Calculate(priceBars);

    return Results.Ok(new
    {
        Symbol = symbol, 
        BarCount = priceBars.Count,
        emaTrend.CurrentPrice,
        emaTrend.Ema20,
        emaTrend.Ema50,
        emaTrend.GoldenCross,
        emaTrend.DeathCross
    });
});

app.MapGet("/test-bollinger/{symbol}", async (string symbol, ApplicationDbContext db) =>
{
    var stock = await db.Stocks.FirstOrDefaultAsync(s => s.Symbol == symbol);
    if (stock is null)
    {
        return Results.NotFound($"'{symbol}' stock not found.");
    }
    
    var priceBars = await db.PriceBars
        .Where(p => p.StockId == stock.Id && p.Interval == PriceInterval.Daily)
        .OrderBy(p => p.BarTime)
        .ToListAsync();

    var bollingerCalculator = new BollingerBandsCalculator();
    var bollinger = bollingerCalculator.Calculate(priceBars);

    return Results.Ok(new
    {
        Symbol = symbol,
        BarCount = priceBars.Count,
        bollinger.CurrentPrice,
        bollinger.UpperBand,
        bollinger.MiddleBand,
        bollinger.LowerBand,
    });
});

app.MapGet("/test-stochastic/{symbol}", async (string symbol, ApplicationDbContext db) =>
{
    var stock = await db.Stocks.FirstOrDefaultAsync(s => s.Symbol == symbol);
    if (stock is null)
    {
        return Results.NotFound($"'{symbol}' bulunamadı.");
    }

    var priceBars = await db.PriceBars
        .Where(p => p.StockId == stock.Id && p.Interval == PriceInterval.Daily)
        .OrderBy(p => p.BarTime)
        .ToListAsync();

    var stochasticCalculator = new StochasticOscillatorCalculator();
    var stochastic = stochasticCalculator.Calculate(priceBars);

    return Results.Ok(new
    {
        Symbol = symbol,
        BarCount = priceBars.Count,
        stochastic.PercentK,
        stochastic.PercentD
    });
});

app.MapPost("/test-sync/{symbol}", async (string symbol, [FromServices] IPriceDataService priceDataService) =>
{
    var count = await priceDataService.SyncHistoricalDataAsync(
        symbol,
        DateTimeOffset.UtcNow.AddDays(-90),
        DateTimeOffset.UtcNow);

    return Results.Ok(new { InsertedCount = count });
});

app.Run();