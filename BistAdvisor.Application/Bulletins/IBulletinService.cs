using BistAdvisor.Domain.Entities;

namespace BistAdvisor.Application.Bulletins;

public interface IBulletinService
{
    Task<DailyBulletin> GenerateDailyBulletinAsync(DateOnly bulletinDate, CancellationToken cancellationToken = default);
}