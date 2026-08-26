using BistAdvisor.Application.MarketData;
using BistAdvisor.Application.Indicators;
using BistAdvisor.Application.Jobs;
using BistAdvisor.Domain.Entities;
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
            _logger.LogInformation("Data sync cycle started at: {Time}", DateTimeOffset.UtcNow);

            var intervalMinutes = await GetDataFetchIntervalAsync(stoppingToken);

            await RunCycleAsync(stoppingToken);

            _logger.LogInformation("Data sync cycle completed at: {Time}", DateTimeOffset.UtcNow);

            await Task.Delay(TimeSpan.FromMinutes(intervalMinutes), stoppingToken);
        }
    }
    
    private async Task<int> GetDataFetchIntervalAsync(CancellationToken stoppingToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var setting = await dbContext.ApplicationSettings
            .FirstOrDefaultAsync(s => s.Key == "Job.DataFetchIntervalMinutes", stoppingToken);

        if (setting is not null && int.TryParse(setting.Value, out var minutes))
        {
            return minutes;
        }

        return 15;
    }

    private async Task RunCycleAsync(CancellationToken stoppingToken)
     {
         using var scope = _scopeFactory.CreateScope();
 
         var jobLockService = scope.ServiceProvider.GetRequiredService<IJobLockService>();
         const string jobName = "DataSyncAndSignalCalculation";
 
         var lockAcquired = await jobLockService.TryAcquireLockAsync(jobName, stoppingToken);
 
         if (!lockAcquired)
         {
             _logger.LogWarning("Could not acquire job lock '{JobName}'; another process is already running it. Skipping this cycle.", jobName);
             return;
         }
 
         try
         {
             var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
             var priceDataService = scope.ServiceProvider.GetRequiredService<IPriceDataService>();
             var signalService = scope.ServiceProvider.GetRequiredService<ISignalService>();
             
             var marketHoursService = scope.ServiceProvider.GetRequiredService<IMarketHoursService>();
             var isMarketOpen = await marketHoursService.IsMarketOpenAsync(stoppingToken);

             if (!isMarketOpen)
             {
                 _logger.LogInformation("Market is currently closed (outside BIST trading hours). Proceeding with sync anyway to keep historical data up to date.");
             }
 
             var activeStocks = await dbContext.Stocks
                 .Where(s => s.IsActive)
                 .ToListAsync(stoppingToken);
 
             foreach (var stock in activeStocks)
             {
                 var log = new DataFetchLog
                 {
                     JobName = jobName,
                     StockId = stock.Id,
                     StartedAt = DateTimeOffset.UtcNow,
                     Status = JobStatus.Success
                 };
 
                 try
                 {
                     var insertedCount = await priceDataService.SyncHistoricalDataAsync(
                         stock.Symbol,
                         DateTimeOffset.UtcNow.AddDays(-90),
                         DateTimeOffset.UtcNow,
                         stoppingToken);
 
                     await signalService.CalculateAndSaveSignalAsync(stock.Symbol, stoppingToken);
 
                     log.InsertedRowCount = insertedCount;
                     log.RetrievedRowCount = insertedCount;
                     log.UpdatedRowCount = 0;
                     log.Status = JobStatus.Success;
                     log.CompletedAt = DateTimeOffset.UtcNow;
 
                     _logger.LogInformation("Successfully processed {Symbol}", stock.Symbol);
                 }
                 catch (Exception ex)
                 {
                     log.Status = JobStatus.Failed;
                     log.ErrorMessage = ex.Message;
                     log.CompletedAt = DateTimeOffset.UtcNow;
 
                     _logger.LogError(ex, "Failed to process {Symbol}, continuing with next stock", stock.Symbol);
                 }
                 finally
                 {
                     dbContext.DataFetchLogs.Add(log);
                 }
             }
 
             await dbContext.SaveChangesAsync(stoppingToken);
         }
         finally
         {
             await jobLockService.ReleaseLockAsync(jobName, stoppingToken);
         }
     }
 }