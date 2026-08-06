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
    }
}