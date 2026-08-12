namespace BistAdvisor.Application.Dtos;

public class SignalDto
{
    public string Symbol { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string SignalType { get; set; }
    public decimal? TotalScore { get; set; }
    public decimal? ConfidenceRate { get; set; }
    public string? Explanation { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}