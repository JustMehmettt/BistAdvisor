namespace BistAdvisor.Domain.Entities;

public enum PriceInterval
{
    OneMinute,
    FiveMinutes,
    FifteenMinutes,
    OneHour,
    Daily
}

public class PriceBar
{
    public long Id { get; set; }
    
    public int StockId { get; set; }
    public Stock Stock { get; set; } = null!;
    
    public PriceInterval Interval { get; set; }
    public DateTimeOffset BarTime { get; set; }
    
    public decimal OpenPrice { get; set; }
    public decimal HighPrice { get; set; }
    public decimal LowPrice { get; set; }
    public decimal ClosePrice { get; set; }
    public decimal? AdjustedClosePrice { get; set; }
    
    public long Volume { get; set; }
    
    public string DataSource { get; set; } = string.Empty;
    
    public DateTimeOffset ReceivedAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}