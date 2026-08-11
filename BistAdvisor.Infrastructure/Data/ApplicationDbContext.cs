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

            entity.HasOne(s => s.Stock)
                .WithMany()
                .HasForeignKey(s => s.StockId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        
    }
}