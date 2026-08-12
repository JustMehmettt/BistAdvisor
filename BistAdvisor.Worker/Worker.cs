using BistAdvisor.Application.MarketData;
using BistAdvisor.Application.Indicators;
using BistAdvisor.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BistAdvisor.Worker;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    
    public Worker(ILogger<Worker> logger, IServiceScopeFactory scopeFactory)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Data sync cycle started at: {time}", DateTimeOffset.UtcNow);
            
            await RunCycleAsync(stoppingToken);
            
            _logger.LogInformation("Data sync cycle completed at: {time}", DateTimeOffset.UtcNow);
        
            await Task.Delay(TimeSpan.FromMinutes(15), stoppingToken);
        }
    }

    private async Task RunCycleAsync(CancellationToken stoppingToken)
    {
        using var scope = _scopeFactory.CreateScope();
        
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var priceDataService = scope.ServiceProvider.GetRequiredService<IPriceDataService>();
        var signalService = scope.ServiceProvider.GetRequiredService<ISignalService>();
        
        var activeStocks = await dbContext.Stocks
            .Where(s => s.IsActive)
            .ToListAsync(stoppingToken);

        foreach (var stock in activeStocks)
        {
            try
            {
                await priceDataService.SyncHistoricalDataAsync(
                    stock.Symbol,
                    DateTimeOffset.UtcNow.AddDays(-90),
                    DateTimeOffset.UtcNow,
                    stoppingToken);
                
                await signalService.CalculateAndSaveSignalAsync(stock.Symbol, stoppingToken);
                
                _logger.LogInformation("Successfully processed {Symbol}", stock.Symbol);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process {Symbol}, continuing with next stock", stock.Symbol);
            }
        }
    }
}