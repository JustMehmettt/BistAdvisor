namespace BistAdvisor.Domain.Entities;

public class IndicatorResult
{
    public long Id { get; set; }
    
    public int StockId { get; set; }
    public Stock Stock { get; set; } = null!;
    
    public DateTimeOffset BarTime { get; set; }
    public PriceInterval Interval { get; set; }
    
    public decimal? RsiValue { get; set; }
    public decimal? MacdValue { get; set; }
    public decimal? MacdSignalValue { get; set; }
    public decimal? MacdHistogramValue { get; set; }
    public decimal? Ema20 { get; set; }
    public decimal? Ema50 { get; set; }
    public decimal? BollingerUpper { get; set; }
    public decimal? BollingerMiddle { get; set; }
    public decimal? BollingerLower { get; set; }
    public decimal? StochasticK { get; set; }
    public decimal? StochasticD { get; set; }
    public long? AverageVolume20 { get; set; }
    
    public DateTimeOffset CalculatedAt { get; set; }
}