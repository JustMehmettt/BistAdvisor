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
            Create("AEFES", "AEFES.IS", "ANADOLU EFES BİRACILIK ve MALT SANAYİİ A.Ş.", "İmalat", now),
            Create("AKBNK", "AKBNK.IS", "AKBANK T.A.Ş.", "Mali Kuruluşlar", now),
            Create("AKSA", "AKSA.IS", "AKSA AKRİLİK KİMYA SANAYİİ A.Ş.", "İmalat", now),
            Create("AKSEN", "AKSEN.IS", "AKSA ENERJİ ÜRETİM A.Ş.", "Elektrik Gaz ve Su", now),
            Create("ALARK", "ALARK.IS", "ALARKO HOLDİNG A.Ş.", "Mali Kuruluşlar", now),
            Create("ALTNY", "ALTNY.IS", "ALTINAY SAVUNMA TEKNOLOJİLERİ A.Ş.", "Teknoloji", now),
            Create("ANSGR", "ANSGR.IS", "ANADOLU ANONİM TÜRK SİGORTA ŞİRKETİ", "Mali Kuruluşlar", now),
            Create("ARCLK", "ARCLK.IS", "ARÇELİK A.Ş.", "İmalat", now),
            Create("ASELS", "ASELS.IS", "ASELSAN ELEKTRONİK SANAYİ ve TİCARET A.Ş.", "Teknoloji", now),
            Create("ASTOR", "ASTOR.IS", "ASTOR ENERJİ A.Ş.", "İmalat", now),
            Create("BALSU", "BALSU.IS", "BALSU GIDA SANAYİ ve TİCARET A.Ş.", "İmalat", now),
            Create("BERA", "BERA.IS", "BERA HOLDİNG A.Ş.", "Mali Kuruluşlar", now),
            Create("BIMAS", "BIMAS.IS", "BİM BİRLEŞİK MAĞAZALAR A.Ş.", "Toptan ve Perakende Ticaret", now),
            Create("BRSAN", "BRSAN.IS", "BORUSAN BİRLEŞİK BORU FABRİKALARI SANAYİ ve TİCARET A.Ş.", "İmalat", now),
            Create("BRYAT", "BRYAT.IS", "BORUSAN YATIRIM ve PAZARLAMA A.Ş.", "Mali Kuruluşlar", now),
            Create("BSOKE", "BSOKE.IS", "BATIÇİM ÇİMENTO SANAYİ A.Ş.", "İmalat", now),
            Create("BTCIM", "BTCIM.IS", "BATIÇİM BATI ANADOLU SANAYİ ve TİCARET A.Ş.", "İmalat", now),
            Create("CANTE", "CANTE.IS", "ÇAN2 TERMİK A.Ş.", "Elektrik Gaz ve Su", now),
            Create("CCOLA", "CCOLA.IS", "COCA-COLA İÇECEK A.Ş.", "İmalat", now),
            Create("CIMSA", "CIMSA.IS", "ÇİMSA ÇİMENTO SANAYİ ve TİCARET A.Ş.", "İmalat", now),
            Create("CVKMD", "CVKMD.IS", "CVK MADEN İŞLETMELERİ SANAYİ ve TİCARET A.Ş.", "Madencilik ve Taş Ocakçılığı", now),
            Create("CWENE", "CWENE.IS", "CW ENERJİ MÜHENDİSLİK TİCARET ve SANAYİ A.Ş.", "Elektrik Gaz ve Su", now),
            Create("DAPGM", "DAPGM.IS", "DAP GAYRİMENKUL GELİŞTİRME A.Ş.", "İnşaat ve Bayındırlık", now),
            Create("DOAS", "DOAS.IS", "DOĞUŞ OTOMOTİV SERVİS ve TİCARET A.Ş.", "Toptan ve Perakende Ticaret", now),
            Create("DOHOL", "DOHOL.IS", "DOĞAN ŞİRKETLER GRUBU HOLDİNG A.Ş.", "Mali Kuruluşlar", now),
            Create("DSTKF", "DSTKF.IS", "DESTEK FİNANS FAKTORİNG A.Ş.", "Mali Kuruluşlar", now),
            Create("ECILC", "ECILC.IS", "EİS ECZACIBAŞI İLAÇ SINAİ ve FİNANSAL YATIRIMLAR SANAYİ ve TİCARET A.Ş.", "Mali Kuruluşlar", now),
            Create("EFOR", "EFOR.IS", "EFOR YATIRIM SANAYİ TİCARET A.Ş.", "İmalat", now),
            Create("EKGYO", "EKGYO.IS", "EMLAK KONUT GAYRİMENKUL YATIRIM ORTAKLIĞI A.Ş.", "Mali Kuruluşlar", now),
            Create("ENERY", "ENERY.IS", "ENERYA ENERJİ A.Ş.", "Elektrik Gaz ve Su", now),
            Create("ENJSA", "ENJSA.IS", "ENERJİSA ENERJİ A.Ş.", "Elektrik Gaz ve Su", now),
            Create("ENKAI", "ENKAI.IS", "ENKA İNŞAAT ve SANAYİ A.Ş.", "İnşaat ve Bayındırlık", now),
            Create("EREGL", "EREGL.IS", "EREĞLİ DEMİR ve ÇELİK FABRİKALARI T.A.Ş.", "İmalat", now),
            Create("ESEN", "ESEN.IS", "ESENBOĞA ELEKTRİK ÜRETİM A.Ş.", "Elektrik Gaz ve Su", now),
            Create("EUPWR", "EUPWR.IS", "EUROPOWER ENERJİ ve OTOMASYON TEKNOLOJİLERİ SANAYİ TİCARET A.Ş.", "İmalat", now),
            Create("EUREN", "EUREN.IS", "EUROPEN ENDÜSTRİ İNŞAAT SANAYİ ve TİCARET A.Ş.", "İmalat", now),
            Create("FENER", "FENER.IS", "FENERBAHÇE FUTBOL A.Ş.", "Eğitim Sağlık Spor ve Eğlence Hizmetleri", now),
            Create("FROTO", "FROTO.IS", "FORD OTOMOTİV SANAYİ A.Ş.", "İmalat", now),
            Create("GARAN", "GARAN.IS", "TÜRKİYE GARANTİ BANKASI A.Ş.", "Mali Kuruluşlar", now),
            Create("GENIL", "GENIL.IS", "GEN İLAÇ ve SAĞLIK ÜRÜNLERİ SANAYİ ve TİCARET A.Ş.", "Toptan ve Perakende Ticaret", now),
            Create("GESAN", "GESAN.IS", "GİRİŞİM ELEKTRİK SANAYİ TAAHHÜT ve TİCARET A.Ş.", "İnşaat ve Bayındırlık", now),
            Create("GLRMK", "GLRMK.IS", "GÜLERMAK AĞIR SANAYİ İNŞAAT ve TAAHHÜT A.Ş.", "İnşaat ve Bayındırlık", now),
            Create("GRSEL", "GRSEL.IS", "GÜR-SEL TURİZM TAŞIMACILIK ve SERVİS TİCARET A.Ş.", "Ulaştırma ve Depolama", now),
            Create("GRTHO", "GRTHO.IS", "GRAINTURK HOLDİNG A.Ş.", "Mali Kuruluşlar", now),
            Create("GSRAY", "GSRAY.IS", "GALATASARAY SPORTİF SINAİ ve TİCARİ YATIRIMLAR A.Ş.", "Eğitim Sağlık Spor ve Eğlence Hizmetleri", now),
            Create("GUBRF", "GUBRF.IS", "GÜBRE FABRİKALARI T.A.Ş.", "İmalat", now),
            Create("HALKB", "HALKB.IS", "TÜRKİYE HALK BANKASI A.Ş.", "Mali Kuruluşlar", now),
            Create("HEKTS", "HEKTS.IS", "HEKTAŞ TİCARET T.A.Ş.", "İmalat", now),
            Create("IEYHO", "IEYHO.IS", "IŞIKLAR ENERJİ ve YAPI HOLDİNG A.Ş.", "Mali Kuruluşlar", now),
            Create("ISCTR", "ISCTR.IS", "TÜRKİYE İŞ BANKASI A.Ş.", "Mali Kuruluşlar", now),
            Create("ISMEN", "ISMEN.IS", "İŞ YATIRIM MENKUL DEĞERLER A.Ş.", "Mali Kuruluşlar", now),
            Create("IZENR", "IZENR.IS", "İZDEMİR ENERJİ ELEKTRİK ÜRETİM A.Ş.", "Elektrik Gaz ve Su", now),
            Create("KCHOL", "KCHOL.IS", "KOÇ HOLDİNG A.Ş.", "Mali Kuruluşlar", now),
            Create("KLRHO", "KLRHO.IS", "KİLER HOLDİNG A.Ş.", "Mali Kuruluşlar", now),
            Create("KRDMD", "KRDMD.IS", "KARDEMİR KARABÜK DEMİR ÇELİK SANAYİ ve TİCARET A.Ş.", "İmalat", now),
            Create("KTLEV", "KTLEV.IS", "KATILIMEVİM TASARRUF FİNANSMAN A.Ş.", "Mali Kuruluşlar", now),
            Create("KUYAS", "KUYAS.IS", "KUYAŞ YATIRIM A.Ş.", "İnşaat ve Bayındırlık", now),
            Create("MAGEN", "MAGEN.IS", "MARGÜN ENERJİ ÜRETİM SANAYİ ve TİCARET A.Ş.", "Elektrik Gaz ve Su", now),
            Create("MAVI", "MAVI.IS", "MAVİ GİYİM SANAYİ ve TİCARET A.Ş.", "Toptan ve Perakende Ticaret", now),
            Create("MGROS", "MGROS.IS", "MİGROS TİCARET A.Ş.", "Toptan ve Perakende Ticaret", now),
            Create("MIATK", "MIATK.IS", "MİA TEKNOLOJİ A.Ş.", "Teknoloji", now),
            Create("MPARK", "MPARK.IS", "MLP SAĞLIK HİZMETLERİ A.Ş.", "Eğitim Sağlık Spor ve Eğlence Hizmetleri", now),
            Create("OBAMS", "OBAMS.IS", "OBA MAKARNACILIK SANAYİ ve TİCARET A.Ş.", "İmalat", now),
            Create("ODAS", "ODAS.IS", "ODAŞ ELEKTRİK ÜRETİM SANAYİ TİCARET A.Ş.", "Elektrik Gaz ve Su", now),
            Create("ODINE", "ODINE.IS", "ODİNE SOLUTİONS TEKNOLOJİ TİCARET ve SANAYİ A.Ş.", "Teknoloji", now),
            Create("OTKAR", "OTKAR.IS", "OTOKAR OTOMOTİV ve SAVUNMA SANAYİ A.Ş.", "İmalat", now),
            Create("OYAKC", "OYAKC.IS", "OYAK ÇİMENTO FABRİKALARI A.Ş.", "İmalat", now),
            Create("PAHOL", "PAHOL.IS", "PASİFİK HOLDİNG A.Ş.", "Mali Kuruluşlar", now),
            Create("PASEU", "PASEU.IS", "PASİFİK EURASİA LOJİSTİK DIŞ TİCARET A.Ş.", "Ulaştırma ve Depolama", now),
            Create("PATEK", "PATEK.IS", "PASİFİK TEKNOLOJİ A.Ş.", "Teknoloji", now),
            Create("PETKM", "PETKM.IS", "PETKİM PETROKİMYA HOLDİNG A.Ş.", "İmalat", now),
            Create("PGSUS", "PGSUS.IS", "PEGASUS HAVA TAŞIMACILIĞI A.Ş.", "Ulaştırma ve Depolama", now),
            Create("PSGYO", "PSGYO.IS", "PASİFİK GAYRİMENKUL YATIRIM ORTAKLIĞI A.Ş.", "Mali Kuruluşlar", now),
            Create("QUAGR", "QUAGR.IS", "QUA GRANITE HAYAL YAPI ve ÜRÜNLERİ SANAYİ TİCARET A.Ş.", "İmalat", now),
            Create("RALYH", "RALYH.IS", "RAL YATIRIM HOLDİNG A.Ş.", "Mali Kuruluşlar", now),
            Create("REEDR", "REEDR.IS", "REEDER TEKNOLOJİ SANAYİ ve TİCARET A.Ş.", "Teknoloji", now),
            Create("SAHOL", "SAHOL.IS", "HACI ÖMER SABANCI HOLDİNG A.Ş.", "Mali Kuruluşlar", now),
            Create("SARKY", "SARKY.IS", "SARKUYSAN ELEKTROLİTİK BAKIR SANAYİ ve TİCARET A.Ş.", "İmalat", now),
            Create("SASA", "SASA.IS", "SASA POLYESTER SANAYİ A.Ş.", "İmalat", now),
            Create("SISE", "SISE.IS", "TÜRKİYE ŞİŞE ve CAM FABRİKALARI A.Ş.", "Mali Kuruluşlar", now),
            Create("SKBNK", "SKBNK.IS", "ŞEKERBANK T.A.Ş.", "Mali Kuruluşlar", now),
            Create("SOKM", "SOKM.IS", "ŞOK MARKETLER TİCARET A.Ş.", "Toptan ve Perakende Ticaret", now),
            Create("TAVHL", "TAVHL.IS", "TAV HAVALİMANLARI HOLDİNG A.Ş.", "Mali Kuruluşlar", now),
            Create("TCELL", "TCELL.IS", "TURKCELL İLETİŞİM HİZMETLERİ A.Ş.", "Bilgi ve İletişim", now),
            Create("THYAO", "THYAO.IS", "TÜRK HAVA YOLLARI A.O.", "Ulaştırma ve Depolama", now),
            Create("TKFEN", "TKFEN.IS", "TEKFEN HOLDİNG A.Ş.", "Mali Kuruluşlar", now),
            Create("TOASO", "TOASO.IS", "TOFAŞ TÜRK OTOMOBİL FABRİKASI A.Ş.", "İmalat", now),
            Create("TRALT", "TRALT.IS", "TÜRK ALTIN İŞLETMELERİ A.Ş.", "Madencilik ve Taş Ocakçılığı", now),
            Create("TRENJ", "TRENJ.IS", "TR DOĞAL ENERJİ KAYNAKLARI ARAŞTIRMA ve ÜRETİM A.Ş.", "Madencilik ve Taş Ocakçılığı", now),
            Create("TRMET", "TRMET.IS", "TR ANADOLU METAL MADENCİLİK İŞLETMELERİ A.Ş.", "Madencilik ve Taş Ocakçılığı", now),
            Create("TSKB", "TSKB.IS", "TÜRKİYE SINAİ KALKINMA BANKASI A.Ş.", "Mali Kuruluşlar", now),
            Create("TTKOM", "TTKOM.IS", "TÜRK TELEKOMÜNİKASYON A.Ş.", "Bilgi ve İletişim", now),
            Create("TUKAS", "TUKAS.IS", "TUKAŞ GIDA SANAYİ ve TİCARET A.Ş.", "İmalat", now),
            Create("TUPRS", "TUPRS.IS", "TÜPRAŞ-TÜRKİYE PETROL RAFİNERİLERİ A.Ş.", "İmalat", now),
            Create("TURSG", "TURSG.IS", "TÜRKİYE SİGORTA A.Ş.", "Mali Kuruluşlar", now),
            Create("ULKER", "ULKER.IS", "ÜLKER BİSKÜVİ SANAYİ A.Ş.", "İmalat", now),
            Create("VAKBN", "VAKBN.IS", "TÜRKİYE VAKIFLAR BANKASI T.A.O.", "Mali Kuruluşlar", now),
            Create("VESTL", "VESTL.IS", "VESTEL ELEKTRONİK SANAYİ ve TİCARET A.Ş.", "İmalat", now),
            Create("YKBNK", "YKBNK.IS", "YAPI ve KREDİ BANKASI A.Ş.", "Mali Kuruluşlar", now),
            Create("ZOREN", "ZOREN.IS", "ZORLU ENERJİ ELEKTRİK ÜRETİM A.Ş.", "Elektrik Gaz ve Su", now),
        };

        await context.Stocks.AddRangeAsync(stocks);
        await context.SaveChangesAsync();

        if (!await context.ApplicationSettings.AnyAsync())
        {
            var settingsNow = DateTimeOffset.UtcNow;

            var settings = new List<ApplicationSetting>
            {
                new() { Key = "Weight.Rsi", Value = "0.20", Description = "RSI indicator weight", UpdatedAt = settingsNow },
                new() { Key = "Weight.Macd", Value = "0.25", Description = "MACD indicator weight", UpdatedAt = settingsNow },
                new() { Key = "Weight.Ema", Value = "0.25", Description = "EMA20/EMA50 indicator weight", UpdatedAt = settingsNow },
                new() { Key = "Weight.Bollinger", Value = "0.15", Description = "Bollinger Bands indicator weight", UpdatedAt = settingsNow },
                new() { Key = "Weight.Stochastic", Value = "0.15", Description = "Stochastic Oscillator indicator weight", UpdatedAt = settingsNow },
                new() { Key = "Threshold.StrongBuy", Value = "60", Description = "Minimum score for Strong Buy signal", UpdatedAt = settingsNow },
                new() { Key = "Threshold.Buy", Value = "20", Description = "Minimum score for Buy signal", UpdatedAt = settingsNow },
                new() { Key = "Threshold.Neutral", Value = "-19", Description = "Minimum score for Neutral signal", UpdatedAt = settingsNow },
                new() { Key = "Threshold.Sell", Value = "-59", Description = "Minimum score for Sell signal", UpdatedAt = settingsNow }
            };

            await context.ApplicationSettings.AddRangeAsync(settings);
            await context.SaveChangesAsync();
        }
    }

    private static Stock Create(string symbol, string providerSymbol, string companyName, string? sector,
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