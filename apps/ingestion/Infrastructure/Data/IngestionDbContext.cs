using Ingestion.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Ingestion.Infrastructure.Data;

public class IngestionDbContext : DbContext
{
    public IngestionDbContext(DbContextOptions<IngestionDbContext> options) : base(options) { }

    public DbSet<Currency> Currencies => Set<Currency>();
    public DbSet<Airline> Airlines => Set<Airline>();
    public DbSet<Acquirer> Acquirers => Set<Acquirer>();
    public DbSet<CardBrand> CardBrands => Set<CardBrand>();
    public DbSet<Transaction> Transactions => Set<Transaction>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Currency
        modelBuilder.Entity<Currency>(entity =>
        {
            entity.ToTable("currencies");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.Code).HasMaxLength(3).IsRequired();
            entity.HasIndex(e => e.Code).IsUnique();
            entity.Property(e => e.IsoNumber).HasMaxLength(3).IsRequired();
            entity.Property(e => e.Symbol).HasMaxLength(5).IsRequired();
            entity.Property(e => e.DecimalPoints).IsRequired();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("now()");
        });

        // Airline
        modelBuilder.Entity<Airline>(entity =>
        {
            entity.ToTable("airlines");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.Code).HasMaxLength(20).IsRequired();
            entity.HasIndex(e => e.Code).IsUnique();
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Country).HasMaxLength(2).IsRequired();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("now()");
            entity.HasOne(e => e.Currency).WithMany(c => c.Airlines).HasForeignKey(e => e.CurrencyId);
        });

        // Acquirer
        modelBuilder.Entity<Acquirer>(entity =>
        {
            entity.ToTable("acquirers");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.Code).HasMaxLength(20).IsRequired();
            entity.HasIndex(e => e.Code).IsUnique();
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Country).HasMaxLength(2).IsRequired();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("now()");
            entity.HasOne(e => e.Currency).WithMany(c => c.Acquirers).HasForeignKey(e => e.CurrencyId);
        });

        // CardBrand
        modelBuilder.Entity<CardBrand>(entity =>
        {
            entity.ToTable("card_brands");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.Code).HasMaxLength(20).IsRequired();
            entity.HasIndex(e => e.Code).IsUnique();
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Country).HasMaxLength(2).IsRequired();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("now()");
        });

        // Transaction
        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.ToTable("transactions");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.Code).HasMaxLength(50).IsRequired();
            entity.HasIndex(e => e.Code).IsUnique();
            entity.Property(e => e.MaskedCard).HasMaxLength(19).IsRequired();
            entity.Property(e => e.Amount).IsRequired();
            entity.Property(e => e.TransactionDate).IsRequired();
            entity.Property(e => e.Status).HasMaxLength(20).IsRequired();
            entity.Property(e => e.FlightNumber).HasMaxLength(20);
            entity.Property(e => e.OriginAirport).HasMaxLength(3);
            entity.Property(e => e.DestinationAirport).HasMaxLength(3);
            entity.Property(e => e.PassengerReference).HasMaxLength(100);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("now()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("now()");

            // Indexes for common query patterns
            entity.HasIndex(e => e.AirlineId);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.TransactionDate);

            // Foreign keys
            entity.HasOne(e => e.Airline).WithMany(a => a.Transactions).HasForeignKey(e => e.AirlineId);
            entity.HasOne(e => e.Acquirer).WithMany(a => a.Transactions).HasForeignKey(e => e.AcquirerId);
            entity.HasOne(e => e.CardBrand).WithMany(cb => cb.Transactions).HasForeignKey(e => e.CardBrandId);
            entity.HasOne(e => e.Currency).WithMany(c => c.Transactions).HasForeignKey(e => e.CurrencyId);
        });
    }
}
