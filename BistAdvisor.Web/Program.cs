using BistAdvisor.Application.MarketData;
using BistAdvisor.Infrastructure.Data;
using BistAdvisor.Infrastructure.MarketData;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IMarketDataProvider, MockMarketDataProvider>();

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

app.Run();