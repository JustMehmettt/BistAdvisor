namespace BistAdvisor.Domain.Entities;

public class BulletinItem
{
    public long Id { get; set; }
    
    public long BulletinId { get; set; }
    public DailyBulletin Bulletin { get; set; } = null!;
    
    public int StockId { get; set; }
    public Stock Stock { get; set; } = null!;
    
    public int Rank { get; set; }
    public SignalType SignalType { get; set; }
    public decimal? TotalScore { get; set; }
    public decimal? ConfidenceRate { get; set; }
    public decimal? LastPrice  { get; set; }
    public decimal? DailyChangeRate { get; set; }
    public string ReasonText { get; set; } = String.Empty;
    
    public DateTimeOffset CreatedAt  { get; set; }
}