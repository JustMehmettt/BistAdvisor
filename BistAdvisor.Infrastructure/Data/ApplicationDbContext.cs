using BistAdvisor.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BistAdvisor.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Stock> Stocks => Set<Stock>();
    public DbSet<PriceBar> PriceBars => Set<PriceBar>();
    public DbSet<IndicatorResult> IndicatorResults => Set<IndicatorResult>();
    public DbSet<SignalSnapshot> SignalSnapshots => Set<SignalSnapshot>();
    public DbSet<SignalChange> SignalChanges => Set<SignalChange>();
    public DbSet<DailyBulletin> DailyBulletins => Set<DailyBulletin>();
    public DbSet<BulletinItem> BulletinItems => Set<BulletinItem>();
    public DbSet<DataFetchLog> DataFetchLogs => Set<DataFetchLog>();
    public DbSet<MarketDataRawLog> MarketDataRawLogs => Set<MarketDataRawLog>();
    public DbSet<ApplicationSetting> ApplicationSettings => Set<ApplicationSetting>();
    public DbSet<JobLock> JobLocks => Set<JobLock>();
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Stock>(entity =>
        {
            entity.HasIndex(s => s.Symbol).IsUnique();
            entity.Property(s => s.Symbol).HasMaxLength(20).IsRequired();
            entity.Property(s => s.ProviderSymbol).HasMaxLength(20).IsRequired();
            entity.Property(s => s.CompanyName).HasMaxLength(200).IsRequired();
            entity.Property(s => s.Sector).HasMaxLength(100);
            entity.Property(s => s.Market).HasMaxLength(20).IsRequired();
        });

        modelBuilder.Entity<PriceBar>(entity =>
        {
            entity.HasIndex(p => new { p.StockId, p.Interval, p.BarTime }).IsUnique();

            entity.Property(p => p.OpenPrice).HasColumnType("decimal(18,4)");
            entity.Property(p => p.HighPrice).HasColumnType("decimal(18,4)");
            entity.Property(p => p.LowPrice).HasColumnType("decimal(18,4)");
            entity.Property(p => p.ClosePrice).HasColumnType("decimal(18,4)");
            entity.Property(p => p.AdjustedClosePrice).HasColumnType("decimal(18,4)");

            entity.Property(p => p.DataSource).HasMaxLength(50).IsRequired();

            entity.HasOne(p => p.Stock)
                .WithMany(s => s.PriceBars)
                .HasForeignKey(p => p.StockId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<IndicatorResult>(entity =>
        {
            entity.HasIndex(i => new {i.StockId, i.Interval, i.BarTime}).IsUnique();
            
            entity.Property(i => i.RsiValue).HasColumnType("decimal(18,4)");
            entity.Property(i => i.MacdValue).HasColumnType("decimal(18,4)");
            entity.Property(i => i.MacdSignalValue).HasColumnType("decimal(18,4)");
            entity.Property(i => i.MacdHistogramValue).HasColumnType("decimal(18,4)");
            entity.Property(i => i.Ema20).HasColumnType("decimal(18,4)");
            entity.Property(i => i.Ema50).HasColumnType("decimal(18,4)");
            entity.Property(i => i.BollingerUpper).HasColumnType("decimal(18,4)");
            entity.Property(i => i.BollingerMiddle).HasColumnType("decimal(18,4)");
            entity.Property(i => i.BollingerLower).HasColumnType("decimal(18,4)");
            entity.Property(i => i.StochasticK).HasColumnType("decimal(18,4)");
            entity.Property(i => i.StochasticD).HasColumnType("decimal(18,4)");

            entity.HasOne(i => i.Stock)
                .WithMany()
                .HasForeignKey(i => i.StockId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        
        modelBuilder.Entity<SignalSnapshot>(entity =>
        {
            entity.Property(s => s.TotalScore).HasColumnType("decimal(6,2)");
            entity.Property(s => s.ConfidenceRate).HasColumnType("decimal(5,2)");
            entity.Property(s => s.AlgorithmVersion).HasMaxLength(20).IsRequired();
            entity.Property(s => s.SettingsSnapshot).HasColumnType("nvarchar(max)");

            entity.HasOne(s => s.Stock)
                .WithMany()
                .HasForeignKey(s => s.StockId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        
        modelBuilder.Entity<SignalChange>(entity =>
        {
            entity.Property(s => s.PreviousScore).HasColumnType("decimal(6,2)");
            entity.Property(s => s.NewScore).HasColumnType("decimal(6,2)");
            entity.Property(s => s.PreviousConfidenceRate).HasColumnType("decimal(5,2)");
            entity.Property(s => s.NewConfidenceRate).HasColumnType("decimal(5,2)");
            entity.Property(s => s.AlgorithmVersion).HasMaxLength(20).IsRequired();

            entity.HasOne(s => s.Stock)
                .WithMany()
                .HasForeignKey(s => s.StockId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<DailyBulletin>(entity =>
        {
            entity.Property(b => b.Title).HasMaxLength(200).IsRequired();
            entity.Property(b => b.AlgorithmVersion).HasMaxLength(20).IsRequired();
        });

        modelBuilder.Entity<BulletinItem>(entity =>
        {
            entity.Property(i => i.TotalScore).HasColumnType("decimal(6,2)");
            entity.Property(i => i.ConfidenceRate).HasColumnType("decimal(5,2)");
            entity.Property(i => i.LastPrice).HasColumnType("decimal(18,4)");
            entity.Property(i => i.DailyChangeRate).HasColumnType("decimal(8,4)");

            entity.HasOne(i => i.Bulletin)
                .WithMany(b => b.Items)
                .HasForeignKey(i => i.BulletinId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(i => i.Stock)
                .WithMany()
                .HasForeignKey(i => i.StockId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<DataFetchLog>(entity =>
        {
            entity.Property(l => l.JobName).HasMaxLength(100).IsRequired();
            
            entity.HasOne(l => l.Stock)
                .WithMany()
                .HasForeignKey(l => l.StockId)
                .OnDelete(DeleteBehavior.SetNull);
        });
        
        modelBuilder.Entity<MarketDataRawLog>(entity =>
        {
            entity.Property(l => l.ProviderName).HasMaxLength(50).IsRequired();
            entity.Property(l => l.RequestSymbol).HasMaxLength(20).IsRequired();
            entity.Property(l => l.RawResponse).HasColumnType("nvarchar(max)");

            entity.HasOne(l => l.Stock)
                .WithMany()
                .HasForeignKey(l => l.StockId)
                .OnDelete(DeleteBehavior.SetNull);
        });
        
        modelBuilder.Entity<ApplicationSetting>(entity =>
        {
            entity.HasIndex(s => s.Key).IsUnique();
            entity.Property(s => s.Key).HasMaxLength(100).IsRequired();
            entity.Property(s => s.Value).HasMaxLength(50).IsRequired();
        });

        modelBuilder.Entity<JobLock>(entity =>
        {
            entity.HasIndex(l => l.JobName).IsUnique();
            entity.Property(l => l.JobName).HasMaxLength(100).IsRequired();
        });
    }
}