using BistAdvisor.Application.MarketData;
using BistAdvisor.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BistAdvisor.Infrastructure.MarketData;

public class MarketHoursService : IMarketHoursService
{
    private static readonly TimeZoneInfo TurkeyTimeZone =
        TimeZoneInfo.FindSystemTimeZoneById(OperatingSystem.IsWindows() ? "Turkey Standard Time" : "Europe/Istanbul");
    
    private readonly ApplicationDbContext _context;

    public MarketHoursService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> IsMarketOpenAsync(CancellationToken cancellationToken = default)
    {
        var settingsDict = await _context.ApplicationSettings
            .Where(s => s.Key == "Market.OpenHour" || s.Key == "Market.CloseHour")
            .ToDictionaryAsync(s => s.Key, s => s.Value, cancellationToken);
        
        var openHour = ParseIntOrDefault(settingsDict, "Market.OpenHour", 10);
        var closeHour = ParseIntOrDefault(settingsDict, "Market.CloseHour", 18);
        
        var nowInTurkey = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TurkeyTimeZone);

        if (nowInTurkey.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
        {
            return false;
        }
        
        return nowInTurkey.Hour >= openHour && nowInTurkey.Hour < closeHour;
    }

    private static int ParseIntOrDefault(Dictionary<string, string> settings, string key, int defaultVolume)
    {
        if (settings.TryGetValue(key, out var volume) && int.TryParse(volume, out var parsed))
        {
            return parsed;
        }
        
        return defaultVolume;
    }
}