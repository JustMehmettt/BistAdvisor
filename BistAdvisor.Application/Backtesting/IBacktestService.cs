using BistAdvisor.Application.Dtos;

namespace BistAdvisor.Application.Backtesting;

public interface IBacktestService
{
    Task<BacktestResultDto> RunBacktestAsync(string symbol, DateOnly fromDate, DateOnly toDate, CancellationToken cancellationToken = default);
}