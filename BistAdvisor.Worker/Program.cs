using BistAdvisor.Application.Indicators;
using BistAdvisor.Application.Jobs;
using BistAdvisor.Application.MarketData;
using BistAdvisor.Infrastructure.Data;
using BistAdvisor.Infrastructure.Indicators;
using BistAdvisor.Infrastructure.Jobs;
using BistAdvisor.Infrastructure.MarketData;
using BistAdvisor.Worker;
using Microsoft.EntityFrameworkCore;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", Serilog.Events.LogEventLevel.Warning)
    .WriteTo.Console()
    .WriteTo.File("logs/bistadvisor-worker-.log", rollingInterval: RollingInterval.Day, retainedFileCountLimit: 14)
    .CreateLogger();

try
{
    var builder = Host.CreateApplicationBuilder(args);
    builder.Services.AddSerilog();

    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

    builder.Services.AddScoped<IMarketDataProvider, YahooMarketDataProvider>();
    builder.Services.AddScoped<IPriceDataService, PriceDataService>();
    builder.Services.AddScoped<ISignalService, SignalService>();
    builder.Services.AddScoped<IJobLockService, JobLockService>();

    builder.Services.AddHostedService<Worker>();

    var host = builder.Build();
    host.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Worker terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}