namespace BistAdvisor.Domain.Entities;

public enum BulletinStatus
{
    Draft,
    Active,
    Cancelled,
    Revised
}

public class DailyBulletin
{
    public long Id { get; set; }
    
    public DateOnly BulletinDate { get; set; }
    public string Title { get; set; }
    public string Summary { get; set; }
    public string Content { get; set; }
    public BulletinStatus Status { get; set; }
    
    public DateTimeOffset GeneratedAt  { get; set; }
    public DateTimeOffset? PublishedAt  { get; set; }
    public string AlgorithmVersion { get; set; } = "v1.0";

    public ICollection<BulletinItem> Items { get; set; } = new List<BulletinItem>();
}