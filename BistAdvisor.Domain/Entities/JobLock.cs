namespace BistAdvisor.Domain.Entities;

public class JobLock
{
    public int Id { get; set; }
    public string JobName { get; set; } = string.Empty;
    public DateTimeOffset AcquiredAt { get; set; }
}