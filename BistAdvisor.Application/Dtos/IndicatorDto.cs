using BistAdvisor.Domain.Entities;

namespace BistAdvisor.Application.Dtos;

public class IndicatorDto
{
    public DateTimeOffset BarTime { get; set; }
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
}

public class PriceDto
{
    public DateTimeOffset BarTime { get; set; }
    public decimal Open { get; set; }
    public decimal High { get; set; }
    public decimal Low { get; set; }
    public decimal Close { get; set; }
    public long Volume { get; set; }   
}

public class JobStatusDto
{
    public long Id { get; set; }
    public string JobName { get; set; } = string.Empty;
    public string? StockSymbol { get; set; }
    public DateTimeOffset StartedAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public int InsertedRowCount { get; set; }
    public string? ErrorMessage { get; set; }
}

public class DataHealthDto
{
    public bool IsDataSourceAvailable { get; set; }
    public int TotalActiveStocks { get; set; }
    public int StocksWithRecentData { get; set; }
    public int StocksWithStaleData { get; set; }   
    public DateTimeOffset? LastSuccessfulSync { get; set; }  
    public int FailedJobsLast24Hours { get; set; }
}