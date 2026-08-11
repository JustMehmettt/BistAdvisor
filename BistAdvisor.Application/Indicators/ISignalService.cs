using BistAdvisor.Domain.Entities;

namespace BistAdvisor.Application.Indicators;

public interface ISignalService
{
    Task<SignalSnapshot> CalculateAndSaveSignalAsync(string stockSymbol, CancellationToken cancellationToken = default);
}