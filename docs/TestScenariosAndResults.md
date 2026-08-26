# BistAdvisor — Test Senaryoları ve Sonuçları

> Bu belge, projedeki 57 otomatik test senaryosunu kategorilere ayırarak özetler. Testlerin tamamını çalıştırmak için: `dotnet test BistAdvisor.Tests`

**Son çalıştırma sonucu:** 57/57 test başarılı ✅

---

## 1. İndikatör Hesaplama Testleri (23 test)

| Bileşen | Test Edilen Senaryo | Beklenen Sonuç |
|---|---|---|
| RSI | Yetersiz veri (< 15 periyot) | `null` döner |
| RSI | Sürekli artan fiyat serisi | RSI = 100 |
| MACD | Yetersiz veri | Boş `MacdResult` döner |
| MACD | Yeterli veri | MACD çizgisi, sinyal çizgisi, histogram dolu döner |
| MACD | Histogram tutarlılığı | Histogram = MACD Çizgisi − Sinyal Çizgisi |
| EMA20/EMA50 | Yetersiz veri | `null` değerler döner |
| EMA20/EMA50 | Sürekli yükselen trend | EMA20 > EMA50 |
| Bollinger Bantları | Yetersiz veri | `null` değerler döner |
| Bollinger Bantları | Normal veri | Üst Bant > Orta Bant > Alt Bant |
| Bollinger Bantları | Sabit fiyat serisi (standart sapma = 0) | Üç bant da eşit değere yakınsar |
| Stochastic Oscillator | Yetersiz veri | `null` değerler döner |
| Stochastic Oscillator | Normal veri | %K ve %D değerleri 0–100 aralığında |
| Stochastic Oscillator | Sabit fiyat serisi (aralık = 0) | %K = 50 (nötr) |
| İndikatör Puanlama | RSI eşik değerleri (29, 35, 50, 65, 75) | Sırasıyla +2, +1, 0, −1, −2 puan |
| İndikatör Puanlama | MACD yükseliş kesişimi + güçlenen histogram | +2 puan |
| İndikatör Puanlama | MACD çizgisi sinyalin üzerinde (kesişim yok) | +1 puan |
| İndikatör Puanlama | Fiyat her iki EMA'nın üzerinde | +2 puan |
| İndikatör Puanlama | Fiyat her iki EMA'nın altında | −2 puan |
| İndikatör Puanlama | Fiyat alt banda eşit/altında | +2 puan |
| İndikatör Puanlama | Fiyat üst banda eşit/üstünde | −2 puan |
| İndikatör Puanlama | %K, %D'nin üzerinde ve 20'nin altında | +2 puan |
| İndikatör Puanlama | %K, %D'nin altında ve 80'in üzerinde | −2 puan |

## 2. Sinyal Hesaplama ve Sınıflandırma Testleri (9 test)

| Senaryo | Beklenen Sonuç |
|---|---|
| Tüm indikatörler maksimum pozitif (+2) | StrongBuy, TotalScore = 100 |
| Tüm indikatörler maksimum negatif (−2) | StrongSell, TotalScore = −100 |
| Tüm indikatörler nötr (0) | Neutral, TotalScore = 0 |
| 4'ten az indikatör mevcut | InsufficientData, TotalScore = null |
| Ağırlıklı skor kombinasyonları (5 farklı senaryo) | Doğru sınıflandırma (StrongBuy/Buy/Neutral/Sell/StrongSell) |

## 3. Veri Senkronizasyonu ve Kalite Testleri (10 test)

| Senaryo | Beklenen Sonuç |
|---|---|
| İlk veri senkronizasyonu | Tüm barlar başarıyla eklenir |
| Aynı tarih aralığı için ikinci senkronizasyon | 0 yeni kayıt eklenir (mükerrer engelleme) |
| Bilinmeyen sembol ile senkronizasyon | `InvalidOperationException` fırlatılır |
| Geçici hatalardan sonra başarı (2 hata + 1 başarı) | 3 deneme sonunda başarılı, veriler eklenir |
| Sürekli hata (maks. deneme aşılır) | 3 denemeden sonra hata fırlatılır |
| Sürekli hata sonrası günlük kaydı | `MarketDataRawLog`'a başarısız, RetryCount doğru kaydedilir |
| Birden fazla hissede biri hatalı | Hatalı hisse başarısız, öncesi/sonrası hisseler başarıyla işlenir |
| Fiyat verisi hiç yok | `DataUnavailable` sinyali üretilir |
| 60'tan az bar mevcut | `InsufficientData` sinyali üretilir |
| Son bar 7 günden eski | `StaleData` sinyali üretilir |

## 4. Bülten Testleri (3 test)

| Senaryo | Beklenen Sonuç |
|---|---|
| Aynı gün için iki kez bülten oluşturma | İlk bülten "Revised" durumuna geçer, ikincisi "Active" olur |
| Aynı gün için üç kez bülten oluşturma | Her zaman yalnızca 1 aktif bülten bulunur |
| Sinyali değişen/değişmeyen hisse karışımı | Yalnızca sinyali değişen hisse bültene dahil edilir |

## 5. API Entegrasyon Testleri (4 test)

| Senaryo | Beklenen Sonuç |
|---|---|
| Sayfalama (`GET /api/stocks?pageSize=5`) | Doğru sayıda öğe ve toplam sayı döner |
| Sektöre göre filtreleme | Yalnızca ilgili sektördeki hisseler döner |
| Bilinmeyen sembol ile hisse detayı | HTTP 404 Not Found döner |
| Bilinen sembol ile hisse detayı | HTTP 200 ve doğru hisse verisi döner |

## 6. Diğer Servis Testleri (8 test)

`SignalService`, `PriceDataService` gibi servislerin temel davranışlarını (sinyal kaydı oluşturma, önceki/yeni sinyal karşılaştırması, `IndicatorResult` güncelleme mantığı gibi) doğrulayan ek testler.

---

## Test Altyapısı Notları

- **Birim testleri**: `EF Core InMemory` veritabanı kullanır, her test kendi izole veritabanı örneğine sahiptir (`Guid.NewGuid()` ile isimlendirilir), testler arası veri sızıntısı önlenir.
- **Entegrasyon testleri**: `Microsoft.AspNetCore.Mvc.Testing` (`WebApplicationFactory`) kullanır, gerçek bir HTTP sunucusu bellekte ayağa kaldırılır; test ortamında başlangıç veri yükleme (seed) mekanizması devre dışı bırakılır.
- **Retry testleri**: Gerçek bekleme sürelerini (exponential backoff) içerdiği için bu testler diğerlerine göre daha uzun sürer (~20 saniye); bu, işlevsel bir sorun değildir.
