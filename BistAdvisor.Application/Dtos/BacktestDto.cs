namespace BistAdvisor.Application.Dtos;

public class BacktestTradeDto
{
    public string Symbol { get; set; } = string.Empty;
    public DateTimeOffset EntryDate { get; set; }
    public decimal EntryPrice { get; set; }
    public DateTimeOffset? ExitDate { get; set; }
    public decimal? ExitPrice { get; set; }
    public string EntrySignalType { get; set; } = string.Empty;
    public decimal? ReturnPercent { get; set; }
    public bool IsOpen { get; set; }
}

public class BacktestResultDto
{
    public List<BacktestTradeDto> Trades { get; set; } = new();
    public int TotalTrades { get; set; }
    public int WinningTrades { get; set; }
    public decimal WinRate { get; set; }
    public decimal AverageReturnPercent { get; set; }
    public decimal TotalReturnPercent { get; set; }
}