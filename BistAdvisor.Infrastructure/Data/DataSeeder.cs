using BistAdvisor.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BistAdvisor.Infrastructure.Data;

public static class DataSeeder
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        if (await context.Stocks.AnyAsync())
        {
            return;
        }
        
        var now = DateTimeOffset.UtcNow;

        var stocks = new List<Stock>
        {
            Create("THYAO", "THYAO.IS", "Türk Hava Yolları", "Ulaştırma", now),
            Create("AKBNK", "AKBNK.IS", "Akbank", "Bankacılık", now),
            Create("GARAN", "GARAN.IS", "Garanti BBVA", "Bankacılık", now),
            Create("ASELS", "ASELS.IS", "Aselsan", "Savunma", now),
            Create("SASA", "SASA.IS", "Sasa Polyester", "Kimya", now),
            Create("KCHOL", "KCHOL.IS", "Koç Holding", "Holding", now),
            Create("SAHOL", "SAHOL.IS", "Sabancı Holding", "Holding", now),
            Create("EREGL", "EREGL.IS", "Ereğli Demir Çelik", "Metal", now),
            Create("BIMAS", "BIMAS.IS", "BİM Mağazalar", "Perakende", now),
            Create("TUPRS", "TUPRS.IS", "Tüpraş", "Enerji", now),
            Create("PGSUS", "PGSUS.IS", "Pegasus Hava Taşımacılığı", "Ulaştırma", now),
            Create("KOZAL", "KOZAL.IS", "Koza Altın", "Madencilik", now),
            Create("SISE", "SISE.IS", "Şişecam", "Cam Sanayii", now),
            Create("TCELL", "TCELL.IS", "Turkcell", "Telekomünikasyon", now),
            Create("YKBNK", "YKBNK.IS", "Yapı Kredi Bankası", "Bankacılık", now),
            Create("HALKB", "HALKB.IS", "Halkbank", "Bankacılık", now),
            Create("VAKBN", "VAKBN.IS", "VakıfBank", "Bankacılık", now),
            Create("FROTO", "FROTO.IS", "Ford Otosan", "Otomotiv", now),
            Create("TOASO", "TOASO.IS", "Tofaş", "Otomotiv", now),
            Create("ARCLK", "ARCLK.IS", "Arçelik", "Dayanıklı Tüketim", now),
        };
        
        await context.Stocks.AddRangeAsync(stocks);
        await context.SaveChangesAsync();
    }

    private static Stock Create(string symbol, string providerSymbol, string companyName, string sector,
        DateTimeOffset now)
    {
        return new Stock()
        {
            Symbol = symbol,
            ProviderSymbol = providerSymbol,
            CompanyName = companyName,
            Sector = sector,
            Market = "BIST",
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now
        };
    }
}