using BistAdvisor.Application.Backtesting;
using BistAdvisor.Application.Dtos;
using BistAdvisor.Domain.Entities;
using BistAdvisor.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BistAdvisor.Infrastructure.Backtesting;

public class BacktestService : IBacktestService
{
    private readonly ApplicationDbContext _context;

    public BacktestService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<BacktestResultDto> RunBacktestAsync(
        string? symbol, DateOnly fromDate, DateOnly toDate, CancellationToken cancellationToken = default)
    {
        var fromDateTime = fromDate.ToDateTime(TimeOnly.MinValue);
        var toDateTime = toDate.ToDateTime(TimeOnly.MaxValue);

        var stocksQuery = _context.Stocks.Where(s => s.IsActive).AsQueryable();
        if (!string.IsNullOrWhiteSpace(symbol))
        {
            stocksQuery = stocksQuery.Where(s => s.Symbol == symbol);
        }

        var stocks = await stocksQuery.ToListAsync(cancellationToken);
        var allTrades = new List<BacktestTradeDto>();

        foreach (var stock in stocks)
        {
            var snapshots = await _context.SignalSnapshots
                .Where(s => s.StockId == stock.Id
                            && s.Interval == PriceInterval.Daily
                            && s.BarTime >= fromDateTime
                            && s.BarTime <= toDateTime)
                .OrderBy(s => s.BarTime)
                .ToListAsync(cancellationToken);

            var priceBars = await _context.PriceBars
                .Where(p => p.StockId == stock.Id
                            && p.Interval == PriceInterval.Daily
                            && p.BarTime >= fromDateTime
                            && p.BarTime <= toDateTime)
                .ToDictionaryAsync(p => p.BarTime, cancellationToken);

            BacktestTradeDto? openTrade = null;

            foreach (var snapshot in snapshots)
            {
                if (!priceBars.TryGetValue(snapshot.BarTime, out var priceBar))
                {
                    continue;
                }

                var isBuySignal = snapshot.SignalType is SignalType.StrongBuy or SignalType.Buy;
                var isExitSignal =
                    snapshot.SignalType is SignalType.Sell or SignalType.StrongSell or SignalType.Neutral;

                if (openTrade is null && isBuySignal)
                {
                    openTrade = new BacktestTradeDto
                    {
                        Symbol = stock.Symbol,
                        EntryDate = snapshot.BarTime,
                        EntryPrice = priceBar.ClosePrice,
                        EntrySignalType = snapshot.SignalType.ToString(),
                        IsOpen = true
                    };
                }
                else if (openTrade is not null && isExitSignal)
                {
                    openTrade.ExitDate = snapshot.BarTime;
                    openTrade.ExitPrice = priceBar.ClosePrice;
                    openTrade.IsOpen = false;
                    openTrade.ReturnPercent = openTrade.EntryPrice != 0
                        ? Math.Round((priceBar.ClosePrice - openTrade.EntryPrice) / openTrade.EntryPrice * 100, 2)
                        : null;

                    allTrades.Add(openTrade);
                    openTrade = null;
                }
            }

            if (openTrade is not null)
            {
                allTrades.Add(openTrade);
            }
        }

        var closedTrades = allTrades.Where(t => !t.IsOpen && t.ReturnPercent.HasValue).ToList();
        var winningTrades = closedTrades.Count(t => t.ReturnPercent > 0);

        return new BacktestResultDto
        {
            Trades = allTrades.OrderByDescending(t => t.EntryDate).ToList(),
            TotalTrades = closedTrades.Count,
            WinningTrades = winningTrades,
            WinRate = closedTrades.Count > 0
                ? Math.Round((decimal)winningTrades / closedTrades.Count * 100, 2)
                : 0,
            AverageReturnPercent = closedTrades.Count > 0
                ? Math.Round(closedTrades.Average(t => t.ReturnPercent!.Value), 2)
                : 0,
            TotalReturnPercent = closedTrades.Count > 0
                ? Math.Round(closedTrades.Sum(t => t.ReturnPercent!.Value), 2)
                : 0
        };
    }
}