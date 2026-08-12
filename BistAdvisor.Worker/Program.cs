using BistAdvisor.Application.Indicators;
using BistAdvisor.Application.MarketData;
using BistAdvisor.Infrastructure.Data;
using BistAdvisor.Infrastructure.Indicators;
using BistAdvisor.Infrastructure.MarketData;
using BistAdvisor.Worker;
using Microsoft.EntityFrameworkCore;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IMarketDataProvider, MockMarketDataProvider>();
builder.Services.AddScoped<IPriceDataService, PriceDataService>();
builder.Services.AddScoped<ISignalService, SignalService>();

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();