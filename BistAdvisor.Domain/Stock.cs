namespace BistAdvisor.Domain.Entities;

public class Stock
{
    public int Id { get; set; }

    public string Symbol { get; set; } = string.Empty;
    public string ProviderSymbol { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string? Sector { get; set; } = string.Empty;
    public string Market { get; set; } = "BIST";
    
    public bool IsActive {get; set; } = true;
    
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    
    public ICollection<PriceBar> PriceBars { get; set; } = new List<PriceBar>();
}