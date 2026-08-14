namespace BistAdvisor.Domain.Entities;

public enum JobStatus
{
    Success,
    Failed,
    PartialSuccess
}

public class DataFetchLog
{
    public long Id { get; set; }
    
    public string JobName { get; set; } = string.Empty;
    public int? StockId { get; set; }
    public Stock? Stock { get; set; }
    
    public DateTimeOffset StartedAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
    public JobStatus Status { get; set; }
    
    public int RetrievedRowCount { get; set; }
    public int InsertedRowCount { get; set; }
    public int UpdatedRowCount { get; set; }
    public int RetryCount { get; set; }
    public string? ErrorMessage { get; set; }
}