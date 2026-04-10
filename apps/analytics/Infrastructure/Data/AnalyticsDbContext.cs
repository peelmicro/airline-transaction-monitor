using Analytics.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Analytics.Infrastructure.Data;

public class AnalyticsDbContext : DbContext
{
    public AnalyticsDbContext(DbContextOptions<AnalyticsDbContext> options) : base(options) { }

    public DbSet<MetricSnapshot> MetricSnapshots => Set<MetricSnapshot>();
    public DbSet<Alert> Alerts => Set<Alert>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // MetricSnapshot
        modelBuilder.Entity<MetricSnapshot>(entity =>
        {
            entity.ToTable("metric_snapshots");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.Code).HasMaxLength(50).IsRequired();
            entity.HasIndex(e => e.Code).IsUnique();
            entity.Property(e => e.AirlineCode).HasMaxLength(20).IsRequired();
            entity.Property(e => e.WindowMinutes).IsRequired();
            entity.Property(e => e.CurrencyCode).HasMaxLength(3).IsRequired();
            entity.Property(e => e.ErrorRate).HasPrecision(5, 2);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("now()");

            // Composite index for querying metrics per airline per window
            entity.HasIndex(e => new { e.AirlineCode, e.WindowMinutes });
        });

        // Alert
        modelBuilder.Entity<Alert>(entity =>
        {
            entity.ToTable("alerts");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.Code).HasMaxLength(50).IsRequired();
            entity.HasIndex(e => e.Code).IsUnique();
            entity.Property(e => e.AirlineCode).HasMaxLength(20).IsRequired();
            entity.Property(e => e.WindowMinutes).IsRequired();
            entity.Property(e => e.Threshold).HasPrecision(5, 2);
            entity.Property(e => e.ActualValue).HasPrecision(5, 2);
            entity.Property(e => e.Status).HasMaxLength(20).IsRequired();
            entity.Property(e => e.FiredAt).IsRequired();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("now()");

            // Indexes for common query patterns
            entity.HasIndex(e => e.AirlineCode);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.FiredAt);
        });
    }
}
