namespace BistAdvisor.Application.Dtos;

public class SignalChangeDto
{
    public string Symbol { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string PreviousSignalType { get; set; } = string.Empty;
    public string NewSignalType { get; set; } = string.Empty;
    public decimal? PreviousScore { get; set; }
    public decimal? NewScore { get; set; }
    public DateTimeOffset ChangeTime { get; set; }
    public string? ChangeReason { get; set; }
}