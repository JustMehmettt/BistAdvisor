namespace BistAdvisor.Application.Jobs;

public interface IJobLockService
{
    Task<bool> TryAcquireLockAsync(string jobName, CancellationToken cancellationToken = default);
    Task ReleaseLockAsync(string jobName, CancellationToken cancellationToken = default);
}