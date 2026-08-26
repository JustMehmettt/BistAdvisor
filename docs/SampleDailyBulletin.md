# BistAdvisor — Örnek Günlük Bülten

> Bu belge, sistemin `GET /api/bulletins/today` uç noktasından döndürdüğü **gerçek bir çıktının** okunabilir formatta sunulmuş halidir. Ham JSON çıktısı da bu belgenin sonunda, referans amacıyla eklenmiştir.

---

## Günlük Teknik Analiz Bülteni — 26.08.2026

**Durum:** Active
**Oluşturulma:** 26.08.2026 16:31
**Piyasa Özeti:** 0 güçlü al, 14 al, 12 sat, 0 güçlü sat sinyali. Bugün 45 hissenin sinyali değişti.

> Burada yer alan bilgiler yatırım danışmanlığı kapsamında değildir.

---

### 📈 Al Sinyali Üreten Hisseler (14)

| Hisse | Şirket | Fiyat | Günlük Değişim | Skor | Güven Oranı |
|---|---|---|---|---|---|
| ENJSA | Enerjisa Enerji A.Ş. | 113,30 | +2,72% | 50,00 | 68,1% |
| ALARK | Alarko Holding A.Ş. | 107,90 | +5,17% | 45,00 | 64,7% |
| ENKAI | Enka İnşaat ve Sanayi A.Ş. | 86,10 | +3,92% | 45,00 | 63,9% |
| MAVI | Mavi Giyim Sanayi ve Ticaret A.Ş. | 38,70 | +1,63% | 45,00 | 59,4% |
| THYAO | Türk Hava Yolları A.O. | 308,50 | +2,58% | 45,00 | 65,5% |
| EUPWR | Europower Enerji ve Otomasyon Teknolojileri A.Ş. | 96,85 | +2,98% | 40,00 | 64,2% |
| TRENJ | TR Doğal Enerji Kaynakları Araştırma ve Üretim A.Ş. | 105,70 | −1,67% | 30,00 | 46,9% |
| HALKB | Türkiye Halk Bankası A.Ş. | 38,12 | +8,79% | 25,00 | 67,7% |
| ISCTR | Türkiye İş Bankası A.Ş. | 12,75 | +1,27% | 25,00 | 64,6% |
| QUAGR | Qua Granite Hayal Yapı ve Ürünleri A.Ş. | 3,41 | −0,29% | 25,00 | 44,3% |
| ESEN | Esenboğa Elektrik Üretim A.Ş. | 3,59 | +0,56% | 22,50 | 60,4% |
| FENER | Fenerbahçe Futbol A.Ş. | 3,20 | +1,59% | 22,50 | 62,3% |
| FROTO | Ford Otomotiv Sanayi A.Ş. | 80,40 | +1,01% | 22,50 | 62,7% |
| ALTNY | Altınay Savunma Teknolojileri A.Ş. | 18,14 | −3,92% | 20,00 | 48,7% |

**Örnek Gerekçe (ENJSA):**
> ENJSA hissesi 50.00 teknik skor ve %68.1 güven oranıyla Al sinyali üretti.

---

### 📉 Sat Sinyali Üreten Hisseler (12)

| Hisse | Şirket | Fiyat | Günlük Değişim | Skor | Güven Oranı |
|---|---|---|---|---|---|
| BRYAT | Borusan Yatırım ve Pazarlama A.Ş. | 1.725,00 | −1,20% | −20,00 | 45,7% |
| CANTE | Çan2 Termik A.Ş. | 1,21 | −2,42% | −20,00 | 44,7% |
| PETKM | Petkim Petrokimya Holding A.Ş. | 19,83 | −3,36% | −20,00 | 45,7% |
| SKBNK | Şekerbank T.A.Ş. | 6,25 | 0,00% | −20,00 | 45,0% |
| TUKAS | Tukaş Gıda Sanayi ve Ticaret A.Ş. | 2,01 | −1,47% | −20,00 | 46,3% |
| PSGYO | Pasifik Gayrimenkul Yatırım Ortaklığı A.Ş. | 3,42 | −1,72% | −22,50 | 59,4% |
| GRTHO | Grainturk Holding A.Ş. | 222,70 | −1,11% | −25,00 | 59,0% |
| PATEK | Pasifik Teknoloji A.Ş. | 22,04 | −0,81% | −25,00 | 62,0% |
| MAGEN | Margün Enerji Üretim Sanayi ve Ticaret A.Ş. | 32,26 | −7,99% | −27,50 | 58,6% |
| ANSGR | Anadolu Anonim Türk Sigorta Şirketi | 28,06 | −1,06% | −32,50 | 58,6% |
| EFOR | Efor Yatırım Sanayi Ticaret A.Ş. | 18,69 | −3,26% | −32,50 | 59,9% |
| KUYAS | Kuyaş Yatırım A.Ş. | 62,70 | −3,39% | −35,00 | 58,8% |

**Örnek Gerekçe (KUYAS):**
> KUYAS hissesi -35.00 teknik skor ve %58.8 güven oranıyla Sat sinyali üretti.

---

## Ham JSON Çıktısı (Referans)

<details>
<summary>Genişletmek için tıklayın</summary>

```json
{
  "id": 20,
  "bulletinDate": "2026-08-26",
  "title": "Günlük Teknik Analiz Bülteni - 26.08.2026",
  "summary": "0 güçlü al, 14 al, 12 sat, 0 güçlü sat sinyali. Bugün 45 hissenin sinyali değişti.",
  "status": "Active",
  "generatedAt": "2026-08-26T13:31:57.1074998+00:00",
  "items": [
    {
      "symbol": "ENJSA",
      "companyName": "ENERJİSA ENERJİ A.Ş.",
      "signalType": "Buy",
      "totalScore": 50,
      "confidenceRate": 68.06,
      "lastPrice": 113.3,
      "dailyChangeRate": 2.72,
      "reasonText": "ENJSA hissesi 50.00 teknik skor ve %68.1 güven oranıyla Al sinyali üretti.",
      "previousSignalType": null
    }
  ]
}
```

> Not: Yukarıdaki JSON, uzunluğu kısaltmak amacıyla tek bir örnek kalem (`ENJSA`) ile gösterilmiştir. Tam çıktı, sistemin `GET /api/bulletins/today` uç noktasından her zaman güncel olarak alınabilir.

</details>

---

## Bu Örnek Hakkında Notlar

- Bu bülten, sistemin **canlı çalışan hali** tarafından, gerçek Yahoo Finance verisiyle üretilmiştir.
- Bültende yalnızca **o gün sinyali değişen** hisseler yer almaktadır (bilinçli tasarım kararı — bkz. README, "Özellikler" bölümü).
- Sinyal gerekçe metinleri (`reasonText`) ve bülten başlığı/özeti, kullanıcı arayüzünün geri kalanıyla tutarlılık sağlamak amacıyla Türkçe olarak üretilmektedir; API'nin sistemsel hata/durum mesajları ise (örn. HTTP hata cevapları) uluslararası entegrasyon kolaylığı için İngilizce bırakılmıştır.
- `previousSignalType` alanının `null` olması, ilgili hissenin daha önce hiç sinyal değişikliği kaydı olmadığı ya da bu bilginin bültenin oluşturulduğu koşullarda mevcut olmadığı anlamına gelir.
- Bültenin tamamı (Al ve Sat grupları) toplamda **26 hisse** içermektedir; BIST 100'deki diğer 74 hissenin sinyali o gün değişmediği için bültene dahil edilmemiştir.
