using Ingestion.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Ingestion.Infrastructure.Data;

/// <summary>
/// Seeds reference data (currencies, airlines, acquirers, card brands) on startup.
/// Only inserts if the tables are empty — safe to run multiple times.
/// </summary>
public static class IngestionDbSeeder
{
    public static async Task SeedAsync(IngestionDbContext context)
    {
        // Apply pending migrations (skip for non-relational providers like InMemory)
        if (context.Database.IsRelational())
            await context.Database.MigrateAsync();
        else
            await context.Database.EnsureCreatedAsync();

        // Only seed if currencies table is empty (first run)
        if (await context.Currencies.AnyAsync())
            return;

        // ── Currencies ──────────────────────────────────────────────────
        var usd = new Currency { Id = Guid.NewGuid(), Code = "USD", IsoNumber = "840", Symbol = "$", DecimalPoints = 2 };
        var eur = new Currency { Id = Guid.NewGuid(), Code = "EUR", IsoNumber = "978", Symbol = "€", DecimalPoints = 2 };
        var gbp = new Currency { Id = Guid.NewGuid(), Code = "GBP", IsoNumber = "826", Symbol = "£", DecimalPoints = 2 };

        context.Currencies.AddRange(usd, eur, gbp);

        // ── Airlines ────────────────────────────────────────────────────
        context.Airlines.AddRange(
            new Airline { Id = Guid.NewGuid(), Code = "Ryanair", Name = "Ryanair DAC", Country = "IE", CurrencyId = eur.Id },
            new Airline { Id = Guid.NewGuid(), Code = "Iberia", Name = "Iberia Líneas Aéreas de España, S.A.", Country = "ES", CurrencyId = eur.Id },
            new Airline { Id = Guid.NewGuid(), Code = "BritishAirways", Name = "British Airways Plc", Country = "GB", CurrencyId = gbp.Id },
            new Airline { Id = Guid.NewGuid(), Code = "EasyJet", Name = "EasyJet Airline Company Limited", Country = "GB", CurrencyId = gbp.Id },
            new Airline { Id = Guid.NewGuid(), Code = "AmericanAirlines", Name = "American Airlines Group Inc.", Country = "US", CurrencyId = usd.Id },
            new Airline { Id = Guid.NewGuid(), Code = "DeltaAirLines", Name = "Delta Air Lines, Inc.", Country = "US", CurrencyId = usd.Id }
        );

        // ── Acquirers ───────────────────────────────────────────────────
        context.Acquirers.AddRange(
            new Acquirer { Id = Guid.NewGuid(), Code = "ElavonUS", Name = "Elavon, Inc.", Country = "US", CurrencyId = usd.Id },
            new Acquirer { Id = Guid.NewGuid(), Code = "Worldpay", Name = "Worldpay, Inc.", Country = "US", CurrencyId = usd.Id },
            new Acquirer { Id = Guid.NewGuid(), Code = "ElavonEU", Name = "Elavon Financial Services DAC", Country = "IE", CurrencyId = eur.Id },
            new Acquirer { Id = Guid.NewGuid(), Code = "Adyen", Name = "Adyen B.V.", Country = "NL", CurrencyId = eur.Id },
            new Acquirer { Id = Guid.NewGuid(), Code = "Barclays", Name = "Barclays Bank PLC", Country = "GB", CurrencyId = gbp.Id },
            new Acquirer { Id = Guid.NewGuid(), Code = "Santander", Name = "Banco Santander, S.A.", Country = "ES", CurrencyId = eur.Id }
        );

        // ── Card Brands ─────────────────────────────────────────────────
        context.CardBrands.AddRange(
            new CardBrand { Id = Guid.NewGuid(), Code = "Visa", Name = "Visa Inc.", Country = "US" },
            new CardBrand { Id = Guid.NewGuid(), Code = "Mastercard", Name = "Mastercard Incorporated", Country = "US" },
            new CardBrand { Id = Guid.NewGuid(), Code = "Amex", Name = "American Express Company", Country = "US" },
            new CardBrand { Id = Guid.NewGuid(), Code = "UnionPay", Name = "China UnionPay Co., Ltd.", Country = "CN" },
            new CardBrand { Id = Guid.NewGuid(), Code = "JCB", Name = "JCB Co., Ltd.", Country = "JP" }
        );

        await context.SaveChangesAsync();
    }
}
