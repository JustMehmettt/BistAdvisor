using BistAdvisor.Application.Jobs;
using BistAdvisor.Domain.Entities;
using BistAdvisor.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BistAdvisor.Infrastructure.Jobs;

public class JobLockService : IJobLockService
{
    private readonly ApplicationDbContext _context;

    public JobLockService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> TryAcquireLockAsync(string jobName, CancellationToken cancellationToken = default)
    {
        var existingLock = await _context.JobLocks
            .FirstOrDefaultAsync(l => l.JobName == jobName, cancellationToken);

        if (existingLock is not null)
        {
            if (DateTimeOffset.UtcNow - existingLock.AcquiredAt < TimeSpan.FromMinutes(30))
            {
                return false;
            }

            _context.JobLocks.Remove(existingLock);
            await _context.SaveChangesAsync(cancellationToken);
        }

        try
        {
            _context.JobLocks.Add(new JobLock
            {
                JobName = jobName,
                AcquiredAt = DateTimeOffset.UtcNow
            });

            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (DbUpdateException)
        {
            return false;
        }
    }

    public async Task ReleaseLockAsync(string jobName, CancellationToken cancellationToken = default)
    {
        var existingLock = await _context.JobLocks
            .FirstOrDefaultAsync(l => l.JobName == jobName, cancellationToken);

        if (existingLock is not null)
        {
            _context.JobLocks.Remove(existingLock);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}