using Microsoft.EntityFrameworkCore;

namespace Analytics.Infrastructure.Data;

/// <summary>
/// Applies pending migrations on startup.
/// Analytics tables (metric_snapshots, alerts) are populated at runtime by the Analytics Service
/// as it processes transaction events — no static seed data needed.
/// </summary>
public static class AnalyticsDbSeeder
{
    public static async Task SeedAsync(AnalyticsDbContext context)
    {
        // Apply pending migrations (skip for non-relational providers like InMemory)
        if (context.Database.IsRelational())
            await context.Database.MigrateAsync();
        else
            await context.Database.EnsureCreatedAsync();
    }
}
