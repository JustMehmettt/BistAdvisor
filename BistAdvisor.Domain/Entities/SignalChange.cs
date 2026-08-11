namespace BistAdvisor.Domain.Entities;

public class SignalChange
{
    public long Id { get; set; }
    
    public int StockId { get; set; }
    public Stock Stock { get; set; } = null!;
    
    public SignalType PreviousSignalType { get; set; }
    public SignalType NewSignalType { get; set; }
    
    public decimal? PreviousScore { get; set; }
    public decimal? NewScore { get; set; }
    
    public decimal? PreviousConfidenceRate { get; set; }
    public decimal? NewConfidenceRate { get; set; }
    
    public DateTimeOffset ChangeTime { get; set; }
    public string? ChangeReason { get; set; }
    public string? AlgorithmVersion { get; set; } = "v1.0";
    
    public DateTimeOffset CreatedAt { get; set; }
}