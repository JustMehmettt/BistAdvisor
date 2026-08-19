using BistAdvisor.Domain.Entities;

namespace BistAdvisor.Application.Dtos;

public class BulletinDto
{
    public long Id { get; set; }
    public DateOnly BulletinDate { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Summary { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTimeOffset GeneratedAt { get; set; }
    public List<BulletinItemDto> Items { get; set; } = new();
}

public class BulletinItemDto
{
    public string Symbol { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string SignalType { get; set; } = string.Empty;
    public decimal? TotalScore { get; set; }
    public decimal? ConfidenceRate { get; set; }
    public decimal? LastPrice { get; set; }
    public decimal? DailyChangeRate { get; set; }
    public string ReasonText { get; set; } = string.Empty;
}