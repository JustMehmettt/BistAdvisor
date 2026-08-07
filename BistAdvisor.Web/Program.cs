using BistAdvisor.Application.MarketData;
using BistAdvisor.Infrastructure.Data;
using BistAdvisor.Infrastructure.MarketData;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;

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

app.MapPost("/test-sync/{symbol}", async (string symbol, [FromServices] IPriceDataService priceDataService) =>
{
    var count = await priceDataService.SyncHistoricalDataAsync(
        symbol,
        DateTimeOffset.UtcNow.AddDays(-30),
        DateTimeOffset.UtcNow);

    return Results.Ok(new { InsertedCount = count });
});

app.Run();