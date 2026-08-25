using BistAdvisor.Application.Dtos;
using BistAdvisor.Application.Indicators;
using BistAdvisor.Domain.Entities;
using BistAdvisor.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BistAdvisor.Web.Controllers;

[ApiExplorerSettings(IgnoreApi = true)]
public class StocksController : Controller
{
    private readonly ApplicationDbContext _context;

    public StocksController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(string? sector, string? signalType, decimal? minScore, decimal? minConfidence, bool hideStale, string? search, string sortBy = "symbol", string sortDir = "asc", int page = 1)
    {
        const int pageSize = 20;
        var result = await GetStockListAsync(sector, signalType, minScore, minConfidence, hideStale, search, sortBy, sortDir, page, pageSize);

        ViewData["Sectors"] = await _context.Stocks
            .Where(s => s.IsActive && s.Sector != null)
            .Select(s => s.Sector)
            .Distinct()
            .OrderBy(s => s)
            .ToListAsync();

        ViewData["CurrentSector"] = sector;
        ViewData["CurrentSignalType"] = signalType;
        ViewData["CurrentMinScore"] = minScore;
        ViewData["CurrentMinConfidence"] = minConfidence;
        ViewData["CurrentHideStale"] = hideStale;
        ViewData["CurrentSearch"] = search;
        ViewData["CurrentSortBy"] = sortBy;
        ViewData["CurrentSortDir"] = sortDir;
        ViewData["CurrentPage"] = page;
        ViewData["TotalPages"] = result.TotalPages;
        ViewData["TotalCount"] = result.TotalCount;

        return View(result.Items);
    }

    [HttpGet]
    public async Task<IActionResult> Table(string? sector, string? signalType, decimal? minScore, decimal? minConfidence, bool hideStale, string? search, string sortBy = "symbol", string sortDir = "asc", int page = 1)
    {
        const int pageSize = 20;
        var result = await GetStockListAsync(sector, signalType, minScore, minConfidence, hideStale, search, sortBy, sortDir, page, pageSize);

        ViewData["CurrentSortBy"] = sortBy;
        ViewData["CurrentSortDir"] = sortDir;
        ViewData["CurrentSector"] = sector;
        ViewData["CurrentSignalType"] = signalType;
        ViewData["CurrentMinScore"] = minScore;
        ViewData["CurrentMinConfidence"] = minConfidence;
        ViewData["CurrentHideStale"] = hideStale;
        ViewData["CurrentSearch"] = search;
        ViewData["CurrentPage"] = page;
        ViewData["TotalPages"] = result.TotalPages;
        ViewData["TotalCount"] = result.TotalCount;

        return PartialView("_StockTable", result.Items);
    }

    [HttpGet]
    [Route("Stocks/Detail/{symbol}")]
    public async Task<IActionResult> Detail(string symbol)
    {
        var stock = await _context.Stocks.FirstOrDefaultAsync(s => s.Symbol == symbol);

        if (stock is null)
        {
            return NotFound();
        }

        var priceBars = await _context.PriceBars
            .Where(p => p.StockId == stock.Id && p.Interval == PriceInterval.Daily)
            .OrderBy(p => p.BarTime)
            .ToListAsync();

        var signalSnapshots = await _context.SignalSnapshots
            .Where(s => s.StockId == stock.Id)
            .OrderByDescending(s => s.CreatedAt)
            .Take(20)
            .ToListAsync();

        var latestSignal = signalSnapshots.FirstOrDefault();

        var rsiSeries = new RsiCalculator().CalculateSeries(priceBars);
        var macdSeries = new MacdCalculator().CalculateSeries(priceBars);
        var bollingerSeries = new BollingerBandsCalculator().CalculateSeries(priceBars);
        var stochasticSeries = new StochasticOscillatorCalculator().CalculateSeries(priceBars);

        var closePrices = priceBars.Select(p => p.ClosePrice).ToList();
        var ema20Raw = EmaCalculator.CalculateSeries(closePrices, 20);
        var ema50Raw = EmaCalculator.CalculateSeries(closePrices, 50);
        var ema20Padded = PadSeries(ema20Raw, priceBars.Count);
        var ema50Padded = PadSeries(ema50Raw, priceBars.Count);

        var indicatorHistory = new List<IndicatorPointDto>();
        for (var i = 0; i < priceBars.Count; i++)
        {
            indicatorHistory.Add(new IndicatorPointDto
            {
                Date = priceBars[i].BarTime,
                RsiValue = rsiSeries[i],
                MacdValue = macdSeries[i].MacdLine,
                MacdSignalValue = macdSeries[i].SignalLine,
                MacdHistogramValue = macdSeries[i].Histogram,
                Ema20 = ema20Padded[i],
                Ema50 = ema50Padded[i],
                BollingerUpper = bollingerSeries[i].UpperBand,
                BollingerMiddle = bollingerSeries[i].MiddleBand,
                BollingerLower = bollingerSeries[i].LowerBand,
                StochasticK = stochasticSeries[i].PercentK,
                StochasticD = stochasticSeries[i].PercentD
            });
        }

        var detail = new StockDetailDto
        {
            Symbol = stock.Symbol,
            CompanyName = stock.CompanyName,
            Sector = stock.Sector,
            LastPrice = priceBars.Count > 0 ? priceBars[^1].ClosePrice : null,
            SignalType = latestSignal?.SignalType.ToString() ?? "InsufficientData",
            TotalScore = latestSignal?.TotalScore,
            ConfidenceRate = latestSignal?.ConfidenceRate,
            Explanation = latestSignal?.Explanation,
            LastUpdate = latestSignal?.CreatedAt,
            RsiScore = latestSignal?.RsiScore,
            MacdScore = latestSignal?.MacdScore,
            EmaScore = latestSignal?.EmaScore,
            BollingerScore = latestSignal?.BollingerScore,
            StochasticScore = latestSignal?.StochasticScore,
            PriceHistory = priceBars
                .Select(p => new PricePointDto
                {
                    Date = p.BarTime,
                    Open = p.OpenPrice,
                    High = p.HighPrice,
                    Low = p.LowPrice,
                    Close = p.ClosePrice
                })
                .ToList(),
            SignalHistory = signalSnapshots
                .Select(s => new SignalHistoryItemDto
                {
                    Date = s.CreatedAt,
                    SignalType = s.SignalType.ToString(),
                    TotalScore = s.TotalScore
                })
                .ToList(),
            IndicatorHistory = indicatorHistory
        };

        return View(detail);
    }

    private static List<decimal?> PadSeries(List<decimal> series, int totalLength)
    {
        var offset = totalLength - series.Count;
        var padded = new List<decimal?>(new decimal?[Math.Max(offset, 0)]);
        padded.AddRange(series.Select(v => (decimal?)v));
        return padded;
    }

private async Task<PagedResult<StockListItemDto>> GetStockListAsync(
    string? sector, string? signalType, decimal? minScore, decimal? minConfidence, bool hideStale,
    string? search, string sortBy, string sortDir, int page, int pageSize)
{
    var stocks = await _context.Stocks.Where(s => s.IsActive).ToListAsync();

    var allSignals = await _context.SignalSnapshots.ToListAsync();
    var allPrices = await _context.PriceBars
        .Where(p => p.Interval == PriceInterval.Daily)
        .ToListAsync();
    var allIndicators = await _context.IndicatorResults
        .Where(r => r.Interval == PriceInterval.Daily)
        .ToListAsync();

    var latestSignals = allSignals
        .GroupBy(s => s.StockId)
        .Select(g => g.OrderByDescending(s => s.CreatedAt).First())
        .ToDictionary(s => s.StockId);

    var pricesByStock = allPrices
        .GroupBy(p => p.StockId)
        .ToDictionary(g => g.Key, g => g.OrderBy(p => p.BarTime).ToList());

    var latestIndicators = allIndicators
        .GroupBy(r => r.StockId)
        .Select(g => g.OrderByDescending(r => r.BarTime).First())
        .ToDictionary(r => r.StockId);

    var now = DateTimeOffset.UtcNow;

    var combined = stocks.Select(s =>
    {
        var priceHistory = pricesByStock.GetValueOrDefault(s.Id) ?? new List<PriceBar>();
        var lastPrice = priceHistory.Count > 0 ? priceHistory[^1].ClosePrice : (decimal?)null;
        var previousPrice = priceHistory.Count > 1 ? priceHistory[^2].ClosePrice : (decimal?)null;

        decimal? dailyChangeRate = null;
        if (lastPrice.HasValue && previousPrice.HasValue && previousPrice.Value != 0)
        {
            dailyChangeRate = Math.Round((lastPrice.Value - previousPrice.Value) / previousPrice.Value * 100, 2);
        }

        var indicator = latestIndicators.GetValueOrDefault(s.Id);
        var signal = latestSignals.GetValueOrDefault(s.Id);

        string? macdStatus = null;
        if (indicator?.MacdValue.HasValue == true && indicator.MacdSignalValue.HasValue)
        {
            macdStatus = indicator.MacdValue > indicator.MacdSignalValue ? "Yükseliş" : "Düşüş";
        }

        string? emaTrend = null;
        if (indicator?.Ema20.HasValue == true && indicator.Ema50.HasValue && lastPrice.HasValue)
        {
            emaTrend = lastPrice > indicator.Ema20 && indicator.Ema20 > indicator.Ema50
                ? "Güçlü Yükseliş"
                : lastPrice < indicator.Ema20 && indicator.Ema20 < indicator.Ema50
                    ? "Güçlü Düşüş"
                    : "Karışık";
        }

        var isStale = signal is null || (now - signal.CreatedAt) > TimeSpan.FromDays(1);

        return new
        {
            Stock = s,
            Signal = signal,
            LastPrice = lastPrice,
            DailyChangeRate = dailyChangeRate,
            RsiValue = indicator?.RsiValue,
            MacdStatus = macdStatus,
            EmaTrend = emaTrend,
            IsStale = isStale
        };
    }).AsEnumerable();

    if (!string.IsNullOrWhiteSpace(sector))
    {
        combined = combined.Where(x => x.Stock.Sector == sector);
    }

    if (!string.IsNullOrWhiteSpace(search))
    {
        combined = combined.Where(x =>
            x.Stock.Symbol.Contains(search, StringComparison.OrdinalIgnoreCase) ||
            x.Stock.CompanyName.Contains(search, StringComparison.OrdinalIgnoreCase));
    }

    if (!string.IsNullOrWhiteSpace(signalType) &&
        Enum.TryParse<Domain.Entities.SignalType>(signalType, true, out var parsedType))
    {
        combined = combined.Where(x => x.Signal != null && x.Signal.SignalType == parsedType);
    }

    if (minScore.HasValue)
    {
        combined = combined.Where(x => x.Signal != null && x.Signal.TotalScore >= minScore.Value);
    }

    if (minConfidence.HasValue)
    {
        combined = combined.Where(x => x.Signal != null && x.Signal.ConfidenceRate >= minConfidence.Value);
    }

    if (hideStale)
    {
        combined = combined.Where(x => !x.IsStale);
    }

    var sortedResults = SortStocks(
        combined.Select(x => new StockListItemDto
        {
            Symbol = x.Stock.Symbol,
            CompanyName = x.Stock.CompanyName,
            Sector = x.Stock.Sector,
            LastPrice = x.LastPrice,
            DailyChangeRate = x.DailyChangeRate,
            RsiValue = x.RsiValue,
            MacdStatus = x.MacdStatus,
            EmaTrend = x.EmaTrend,
            SignalType = x.Signal?.SignalType.ToString() ?? "InsufficientData",
            TotalScore = x.Signal?.TotalScore,
            ConfidenceRate = x.Signal?.ConfidenceRate,
            LastUpdate = x.Signal?.CreatedAt
        }).ToList(),
        sortBy, sortDir);
    
    var totalCount = sortedResults.Count;
    
    var pagedItems = sortedResults
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .ToList();

    return new PagedResult<StockListItemDto>
    {
        Items = pagedItems,
        Page = page,
        PageSize = pageSize,
        TotalCount = totalCount
    };
}

private static List<StockListItemDto> SortStocks(List<StockListItemDto> items, string sortBy, string sortDir)
{
    IEnumerable<StockListItemDto> sorted = sortBy.ToLower() switch
    {
        "symbol" => sortDir == "desc" ? items.OrderByDescending(x => x.Symbol) : items.OrderBy(x => x.Symbol),
        "companyname" => sortDir == "desc" ? items.OrderByDescending(x => x.CompanyName) : items.OrderBy(x => x.CompanyName),
        "sector" => sortDir == "desc" ? items.OrderByDescending(x => x.Sector) : items.OrderBy(x => x.Sector),
        "lastprice" => sortDir == "desc" ? items.OrderByDescending(x => x.LastPrice) : items.OrderBy(x => x.LastPrice),
        "dailychangerate" => sortDir == "desc" ? items.OrderByDescending(x => x.DailyChangeRate) : items.OrderBy(x => x.DailyChangeRate),
        "rsivalue" => sortDir == "desc" ? items.OrderByDescending(x => x.RsiValue) : items.OrderBy(x => x.RsiValue),
        "totalscore" => sortDir == "desc" ? items.OrderByDescending(x => x.TotalScore) : items.OrderBy(x => x.TotalScore),
        "confidencerate" => sortDir == "desc" ? items.OrderByDescending(x => x.ConfidenceRate) : items.OrderBy(x => x.ConfidenceRate),
        "signaltype" => sortDir == "desc" ? items.OrderByDescending(x => x.SignalType) : items.OrderBy(x => x.SignalType),
        "lastupdate" => sortDir == "desc" ? items.OrderByDescending(x => x.LastUpdate) : items.OrderBy(x => x.LastUpdate),
        _ => items.OrderBy(x => x.Symbol)
    };

    return sorted.ToList();
}
}