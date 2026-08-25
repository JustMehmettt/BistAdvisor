using System.Globalization;
using BistAdvisor.Application.Indicators;
using BistAdvisor.Domain.Entities;
using BistAdvisor.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

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

        var now = DateTimeOffset.UtcNow;

        if (priceBars.Count == 0)
        {
            return await SaveInsufficientSignalAsync(stock, Domain.Entities.SignalType.DataUnavailable,
                "Bu hisse senedi için fiyat verisi alınamadı.", now, previousSnapshot, cancellationToken);
        }

        if (priceBars.Count < 60)
        {
            return await SaveInsufficientSignalAsync(stock, Domain.Entities.SignalType.InsufficientData,
                $"Yalnızca {priceBars.Count} adet fiyat verisi mevcut; güvenilir hesaplama için en az 60 adet gerekmektedir.",
                now, previousSnapshot, cancellationToken);
        }

        var lastBarAge = now - priceBars[^1].BarTime;
        if (lastBarAge > TimeSpan.FromDays(7))
        {
            return await SaveInsufficientSignalAsync(
                stock, Domain.Entities.SignalType.StaleData,
                $"Latest available data is from {priceBars[^1].BarTime:yyyy-MM-dd}, which is more than 7 days old.",
                now, previousSnapshot, cancellationToken);
        }

        try
        {
            var settingsDict = await _context.ApplicationSettings.ToDictionaryAsync(s => s.Key, s => s.Value, cancellationToken);

            var rsi = new RsiCalculator(ParseInt(settingsDict, "Period.Rsi", 14)).Calculate(priceBars);
            var macd = new MacdCalculator(
                ParseInt(settingsDict, "Period.MacdFast", 12),
                ParseInt(settingsDict, "Period.MacdSlow", 26),
                ParseInt(settingsDict, "Period.MacdSignal", 9)).Calculate(priceBars);
            var ema = new EmaTrendCalculator(
                ParseInt(settingsDict, "Period.EmaShort", 20),
                ParseInt(settingsDict, "Period.EmaLong", 50)).Calculate(priceBars);
            var bollinger =
                new BollingerBandsCalculator(ParseInt(settingsDict, "Period.Bollinger", 20)).Calculate(priceBars);
            var stochastic = new StochasticOscillatorCalculator(
                ParseInt(settingsDict, "Period.StochasticK", 14),
                ParseInt(settingsDict, "Period.StochasticD", 3)).Calculate(priceBars);

            var rsiScore = IndicatorScoreCalculator.ScoreRsi(rsi);
            var macdScore = IndicatorScoreCalculator.ScoreMacd(macd);
            var emaScore = IndicatorScoreCalculator.ScoreEmaTrend(ema);
            var bollingerScore = IndicatorScoreCalculator.ScoreBollinger(bollinger);
            var stochasticScore = IndicatorScoreCalculator.ScoreStochastic(stochastic);
            
            var weights = new SignalWeights
            {
                RsiWeight = ParseDecimal(settingsDict, "Weight.Rsi", 0.20m),
                MacdWeight = ParseDecimal(settingsDict, "Weight.Macd", 0.25m),
                EmaWeight = ParseDecimal(settingsDict, "Weight.Ema", 0.25m),
                BollingerWeight = ParseDecimal(settingsDict, "Weight.Bollinger", 0.15m),
                StochasticWeight = ParseDecimal(settingsDict, "Weight.Stochastic", 0.15m),
                StrongBuyThreshold = ParseDecimal(settingsDict, "Threshold.StrongBuy", 60m),
                BuyThreshold = ParseDecimal(settingsDict, "Threshold.Buy", 20m),
                NeutralThreshold = ParseDecimal(settingsDict, "Threshold.Neutral", -19m),
                SellThreshold = ParseDecimal(settingsDict, "Threshold.Sell", -59m)
            };

            decimal volumeRatio = 1m;
            if (priceBars.Count >= 20)
            {
                var last20Bars = priceBars.TakeLast(20).ToList();
                var averageVolume20 = (decimal)last20Bars.Average(p => p.Volume);
                var currentVolume = (decimal)priceBars[^1].Volume;

                if (averageVolume20 > 0)
                {
                    volumeRatio = Math.Min(1m, currentVolume / averageVolume20);
                }
            }
            
            var signal = new SignalCalculator(weights).Calculate(
                rsiScore, macdScore, emaScore, bollingerScore, stochasticScore, volumeRatio);
            
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

            var existingIndicatorResult = await _context.IndicatorResults
                .FirstOrDefaultAsync(r => r.StockId == stock.Id && r.Interval == PriceInterval.Daily && r.BarTime == latestBarTime, cancellationToken);

            if (existingIndicatorResult is not null)
            {
                existingIndicatorResult.RsiValue = rsi;
                existingIndicatorResult.MacdValue = macd.MacdLine;
                existingIndicatorResult.MacdSignalValue = macd.SignalLine;
                existingIndicatorResult.MacdHistogramValue = macd.Histogram;
                existingIndicatorResult.Ema20 = ema.Ema20;
                existingIndicatorResult.Ema50 = ema.Ema50;
                existingIndicatorResult.BollingerUpper = bollinger.UpperBand;
                existingIndicatorResult.BollingerMiddle = bollinger.MiddleBand;
                existingIndicatorResult.BollingerLower = bollinger.LowerBand;
                existingIndicatorResult.StochasticK = stochastic.PercentK;
                existingIndicatorResult.StochasticD = stochastic.PercentD;
                existingIndicatorResult.AverageVolume20 = priceBars.Count >= 20
                    ? (long)priceBars.TakeLast(20).Average(p => p.Volume)
                    : null;
                existingIndicatorResult.CalculatedAt = now;
            }
            else
            {
                _context.IndicatorResults.Add(new IndicatorResult
                {
                    StockId = stock.Id,
                    BarTime = latestBarTime,
                    Interval = PriceInterval.Daily,
                    RsiValue = rsi,
                    MacdValue = macd.MacdLine,
                    MacdSignalValue = macd.SignalLine,
                    MacdHistogramValue = macd.Histogram,
                    Ema20 = ema.Ema20,
                    Ema50 = ema.Ema50,
                    BollingerUpper = bollinger.UpperBand,
                    BollingerMiddle = bollinger.MiddleBand,
                    BollingerLower = bollinger.LowerBand,
                    StochasticK = stochastic.PercentK,
                    StochasticD = stochastic.PercentD,
                    AverageVolume20 = priceBars.Count >= 20
                        ? (long)priceBars.TakeLast(20).Average(p => p.Volume)
                        : null,
                    CalculatedAt = now
                });
            }

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
        catch (Exception ex) when (ex is not InvalidOperationException)
        {
            return await SaveInsufficientSignalAsync(
                stock, Domain.Entities.SignalType.CalculationError,
                $"Hesaplama sırasında beklenmeyen bir hata oluştu: {ex.Message}",
                now, previousSnapshot, cancellationToken);
        }
    }

    private async Task<SignalSnapshot> SaveInsufficientSignalAsync(
        Stock stock,
        Domain.Entities.SignalType signalType,
        string reason,
        DateTimeOffset now,
        SignalSnapshot? previousSnapshot,
        CancellationToken cancellationToken)
    {
        var snapshot = new SignalSnapshot
        {
            StockId = stock.Id,
            BarTime = now,
            Interval = PriceInterval.Daily,
            SignalType = signalType,
            Explanation = reason,
            AlgorithmVersion = "v1.0",
            CreatedAt = now
        };

        _context.SignalSnapshots.Add(snapshot);

        if (previousSnapshot is not null && previousSnapshot.SignalType != signalType)
        {
            var change = new SignalChange
            {
                StockId = stock.Id,
                PreviousSignalType = previousSnapshot.SignalType,
                NewSignalType = signalType,
                PreviousScore = previousSnapshot.TotalScore,
                NewScore = null,
                PreviousConfidenceRate = previousSnapshot.ConfidenceRate,
                NewConfidenceRate = null,
                ChangeTime = now,
                ChangeReason = reason,
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

    private static decimal ParseDecimal(Dictionary<string, string> settings, string key, decimal defaultValue)
    {
        if (settings.TryGetValue(key, out var value) &&
            decimal.TryParse(value, System.Globalization.CultureInfo.InvariantCulture, out var parsed))
        {
            return parsed;
        }

        return defaultValue;
    }
    
    private static int ParseInt(Dictionary<string, string> settings, string key, int defaultValue)
    {
        if (settings.TryGetValue(key, out var value) && int.TryParse(value, out var parsed))
        {
            return parsed;
        }

        return defaultValue;
    }
}