using BistAdvisor.Application.Indicators;
using BistAdvisor.Domain.Entities;
using BistAdvisor.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace BistAdvisor.Infrastructure.Indicators;

public class SignalService : ISignalService
{
    private readonly ApplicationDbContext _context;

    public SignalService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<SignalSnapshot> CalculateAndSaveSignalAsync(
        string stockSymbol,
        CancellationToken cancellationToken = default)
    {
        var stock = await _context.Stocks
            .FirstOrDefaultAsync(s => s.Symbol == stockSymbol, cancellationToken);

        if (stock is null)
        {
            throw new InvalidOperationException($"'{stockSymbol}' sembollü hisse veritabanında bulunamadı.");
        }
        
        var previousSnapshot = await _context.SignalSnapshots
            .Where(s => s.StockId == stock.Id)
            .OrderByDescending(s => s.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        var priceBars = await _context.PriceBars
            .Where(p => p.StockId == stock.Id && p.Interval == PriceInterval.Daily)
            .OrderBy(p => p.BarTime)
            .ToListAsync(cancellationToken);

        var rsi = new RsiCalculator().Calculate(priceBars);
        var macd = new MacdCalculator().Calculate(priceBars);
        var ema = new EmaTrendCalculator().Calculate(priceBars);
        var bollinger = new BollingerBandsCalculator().Calculate(priceBars);
        var stochastic = new StochasticOscillatorCalculator().Calculate(priceBars);

        var rsiScore = IndicatorScoreCalculator.ScoreRsi(rsi);
        var macdScore = IndicatorScoreCalculator.ScoreMacd(macd);
        var emaScore = IndicatorScoreCalculator.ScoreEmaTrend(ema);
        var bollingerScore = IndicatorScoreCalculator.ScoreBollinger(bollinger);
        var stochasticScore = IndicatorScoreCalculator.ScoreStochastic(stochastic);

        var signal = new SignalCalculator().Calculate(rsiScore, macdScore, emaScore, bollingerScore, stochasticScore);
        
        var now = DateTimeOffset.UtcNow;
        var latestBarTime = priceBars.Count > 0 ? priceBars[^1].BarTime : now;
        var newSignalType = MapSignalType(signal.SignalType);

        var snapshot = new SignalSnapshot
        {
            StockId = stock.Id,
            BarTime = latestBarTime,
            Interval = PriceInterval.Daily,
            RsiScore = signal.RsiScore,
            MacdScore = signal.MacdScore,
            EmaScore = signal.EmaScore,
            BollingerScore = signal.BollingerScore,
            StochasticScore = signal.StochasticScore,
            TotalScore = signal.TotalScore,
            ConfidenceRate = signal.ConfidenceRate,
            SignalType = newSignalType,
            Explanation = BuildExplanation(stockSymbol, signal, rsi, macd, ema),
            AlgorithmVersion = "v1.0",
            CreatedAt = now
        };

        _context.SignalSnapshots.Add(snapshot);

        if (previousSnapshot is not null && previousSnapshot.SignalType != newSignalType)
        {
            var change = new SignalChange
            {
                StockId = stock.Id,
                PreviousSignalType = previousSnapshot.SignalType,
                NewSignalType = newSignalType,
                PreviousScore = previousSnapshot.TotalScore,
                NewScore = signal.TotalScore,
                PreviousConfidenceRate = previousSnapshot.ConfidenceRate,
                NewConfidenceRate = signal.ConfidenceRate,
                ChangeTime = now,
                ChangeReason = $"Signal changed from {previousSnapshot.SignalType} to {newSignalType}.",
                AlgorithmVersion = "v1.0",
                CreatedAt = now
            };
            
            _context.SignalChanges.Add(change);
        }
        await _context.SaveChangesAsync(cancellationToken);

        return snapshot;
    }

    private static Domain.Entities.SignalType MapSignalType(Application.Indicators.SignalType signalType)
    {
        return signalType switch
        {
            Application.Indicators.SignalType.StrongBuy => Domain.Entities.SignalType.StrongBuy,
            Application.Indicators.SignalType.Buy => Domain.Entities.SignalType.Buy,
            Application.Indicators.SignalType.Neutral => Domain.Entities.SignalType.Neutral,
            Application.Indicators.SignalType.Sell => Domain.Entities.SignalType.Sell,
            Application.Indicators.SignalType.StrongSell => Domain.Entities.SignalType.StrongSell,
            _ => Domain.Entities.SignalType.InsufficientData
        };
    }

    private static string BuildExplanation(
        string symbol,
        SignalResult signal,
        decimal? rsi,
        MacdResult macd,
        EmaTrendResult ema)
    {
        if (signal.SignalType == Application.Indicators.SignalType.InsufficientData)
        {
            return $"Not enough data available to generate a signal for {symbol}.";
        }

        var totalScoreText = signal.TotalScore?.ToString("F2", CultureInfo.InvariantCulture);
        var confidenceText = signal.ConfidenceRate?.ToString("F1", CultureInfo.InvariantCulture);
        var rsiText = rsi?.ToString("F2", CultureInfo.InvariantCulture);

        return $"{symbol} has a technical score of {totalScoreText} with a confidence rate of " +
               $"{confidenceText}%. RSI is at {rsiText}. " +
               $"MACD line is {(macd.MacdLine > macd.SignalLine ? "above" : "below")} the signal line. " +
               $"Current price is {(ema.CurrentPrice > ema.Ema20 ? "above" : "below")} EMA20.";
    }
}