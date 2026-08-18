<#
.SYNOPSIS
    BistAdvisor projesini sıfırdan kurar: bağımlılıkları geri yükler, derler,
    veritabanı migration'larını uygular ve isteğe bağlı olarak testleri çalıştırır.

.DESCRIPTION
    Bu betik, README.md'deki "Kurulum" adımlarını otomatikleştirir.
    Çalıştırmadan önce appsettings.Development.json dosyasının
    (BistAdvisor.Web ve BistAdvisor.Worker altında) doğru bağlantı dizesiyle
    yapılandırılmış olması gerekir.

.EXAMPLE
    .\setup.ps1
    .\setup.ps1 -SkipTests
#>

param(
    [switch]$SkipTests
)

$ErrorActionPreference = "Stop"

function Write-Step($message) {
    Write-Host ""
    Write-Host "==> $message" -ForegroundColor Cyan
}

function Test-CommandExists($command) {
    return $null -ne (Get-Command $command -ErrorAction SilentlyContinue)
}

Write-Host "BistAdvisor Kurulum Betiği" -ForegroundColor Green
Write-Host "===========================" -ForegroundColor Green

# 1. .NET SDK kontrolü
Write-Step ".NET SDK kontrol ediliyor"
if (-not (Test-CommandExists "dotnet")) {
    Write-Host "HATA: .NET SDK bulunamadı. Lütfen https://dotnet.microsoft.com/download/dotnet/8.0 adresinden .NET 8 SDK'sını kurun." -ForegroundColor Red
    exit 1
}
$dotnetVersion = dotnet --version
Write-Host "Bulunan .NET SDK sürümü: $dotnetVersion"

# 2. dotnet-ef aracı kontrolü
Write-Step "dotnet-ef aracı kontrol ediliyor"
$efInstalled = dotnet tool list --global | Select-String "dotnet-ef"
if (-not $efInstalled) {
    Write-Host "dotnet-ef bulunamadı, global olarak kuruluyor..."
    dotnet tool install --global dotnet-ef
} else {
    Write-Host "dotnet-ef zaten kurulu."
}

# 3. appsettings.Development.json kontrolü
Write-Step "Yapılandırma dosyaları kontrol ediliyor"
$webConfigPath = "BistAdvisor.Web/appsettings.Development.json"
$workerConfigPath = "BistAdvisor.Worker/appsettings.json"

if (-not (Test-Path $webConfigPath)) {
    Write-Host "UYARI: $webConfigPath bulunamadı." -ForegroundColor Yellow
    Write-Host "Lütfen BistAdvisor.Web/appsettings.example.json dosyasını kopyalayıp" -ForegroundColor Yellow
    Write-Host "kendi SQL Server bağlantı bilgilerinizle $webConfigPath olarak kaydedin." -ForegroundColor Yellow
    $continue = Read-Host "Yine de devam etmek istiyor musunuz? (e/H)"
    if ($continue -ne "e") {
        exit 1
    }
} else {
    Write-Host "$webConfigPath bulundu."
}

if (-not (Test-Path $workerConfigPath)) {
    Write-Host "UYARI: $workerConfigPath bulunamadı, Worker projesi çalışmayabilir." -ForegroundColor Yellow
} else {
    Write-Host "$workerConfigPath bulundu."
}

# 4. Bağımlılıkları geri yükle
Write-Step "NuGet bağımlılıkları geri yükleniyor (dotnet restore)"
dotnet restore

# 5. Derle
Write-Step "Solution derleniyor (dotnet build)"
dotnet build --no-restore

# 6. Veritabanı migration'larını uygula
Write-Step "Veritabanı migration'ları uygulanıyor (dotnet ef database update)"
dotnet ef database update --project BistAdvisor.Infrastructure --startup-project BistAdvisor.Web

# 7. Testleri çalıştır (isteğe bağlı)
if (-not $SkipTests) {
    Write-Step "Birim testleri çalıştırılıyor (dotnet test)"
    dotnet test BistAdvisor.Tests --no-restore
} else {
    Write-Host ""
    Write-Host "Testler atlandı (-SkipTests parametresi verildi)." -ForegroundColor Yellow
}

Write-Host ""
Write-Host "===========================" -ForegroundColor Green
Write-Host "Kurulum tamamlandı!" -ForegroundColor Green
Write-Host ""
Write-Host "Uygulamayı çalıştırmak için (iki ayrı terminalde):" -ForegroundColor Green
Write-Host "  dotnet run --project BistAdvisor.Web" -ForegroundColor White
Write-Host "  dotnet run --project BistAdvisor.Worker" -ForegroundColor White
Write-Host ""
Write-Host "Web arayüzü:  http://localhost:5010/" -ForegroundColor White
Write-Host "Swagger:      http://localhost:5010/swagger" -ForegroundColor White
