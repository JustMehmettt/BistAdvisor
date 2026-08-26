namespace BistAdvisor.Application.MarketData;

public interface IMarketHoursService
{
    Task<bool> IsMarketOpenAsync(CancellationToken cancellationToken = default);
}