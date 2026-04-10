using Analytics.Domain.Entities;
using Analytics.Infrastructure.Data;
using Common.Events;
using Common.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Analytics.Application.Services;

/// <summary>
/// Core analytics logic: processes incoming transactions, updates rolling metrics,
/// evaluates alert rules, and publishes events.
/// </summary>
public class MetricsService
{
    private readonly AnalyticsDbContext _context;
    private readonly IEventPublisher _publisher;
    private readonly ILogger<MetricsService> _logger;

    // Alert configuration: error rate threshold per window
    private static readonly Dictionary<int, decimal> AlertThresholds = new()
    {
        [1] = 10.0m,   // 10% error rate in 1-minute window
        [5] = 5.0m,    // 5% error rate in 5-minute window
        [60] = 3.0m    // 3% error rate in 1-hour window
    };

    public MetricsService(AnalyticsDbContext context, IEventPublisher publisher, ILogger<MetricsService> logger)
    {
        _context = context;
        _publisher = publisher;
        _logger = logger;
    }

    /// <summary>
    /// Processes a transaction.created event: updates metrics and evaluates alerts.
    /// </summary>
    public async Task ProcessTransactionAsync(TransactionCreatedEvent evt, CancellationToken ct)
    {
        var isError = evt.Status is "declined" or "failed";
        var windows = new[] { 1, 5, 60 };

        foreach (var window in windows)
        {
            var snapshot = await GetOrCreateSnapshotAsync(evt.AirlineCode, window, evt.CurrencyCode, ct);

            // Update counters
            snapshot.TransactionCount++;
            snapshot.TotalVolume += evt.Amount;
            if (isError) snapshot.ErrorCount++;
            snapshot.ErrorRate = snapshot.TransactionCount > 0
                ? Math.Round((decimal)snapshot.ErrorCount / snapshot.TransactionCount * 100, 2)
                : 0;

            // Compute latency (time from transaction to event processing)
            var latencyMs = (int)(DateTime.UtcNow - evt.TransactionDate).TotalMilliseconds;
            if (latencyMs < 0) latencyMs = 0;
            snapshot.LatencyP95Ms = Math.Max(snapshot.LatencyP95Ms, latencyMs);
            snapshot.LatencyP99Ms = Math.Max(snapshot.LatencyP99Ms, latencyMs);

            snapshot.UpdatedAt = DateTime.UtcNow;

            // Publish metrics update
            await _publisher.PublishAsync(NatsSubjects.MetricsUpdated, new MetricsUpdatedEvent
            {
                AirlineCode = snapshot.AirlineCode,
                WindowMinutes = snapshot.WindowMinutes,
                TransactionCount = snapshot.TransactionCount,
                TotalVolume = snapshot.TotalVolume,
                CurrencyCode = snapshot.CurrencyCode,
                ErrorCount = snapshot.ErrorCount,
                ErrorRate = snapshot.ErrorRate,
                LatencyP95Ms = snapshot.LatencyP95Ms,
                LatencyP99Ms = snapshot.LatencyP99Ms,
                Timestamp = DateTime.UtcNow
            }, ct);

            // Evaluate alert rules
            if (AlertThresholds.TryGetValue(window, out var threshold) && snapshot.ErrorRate > threshold)
            {
                await RaiseAlertAsync(snapshot, threshold, ct);
            }
        }

        await _context.SaveChangesAsync(ct);
    }

    private async Task<MetricSnapshot> GetOrCreateSnapshotAsync(string airlineCode, int window, string currencyCode, CancellationToken ct)
    {
        // Check both DB and local tracked entities (not yet saved)
        var snapshot = await _context.MetricSnapshots
            .FirstOrDefaultAsync(s => s.AirlineCode == airlineCode && s.WindowMinutes == window, ct);

        if (snapshot is not null) return snapshot;

        // Also check locally added (unsaved) entities
        snapshot = _context.MetricSnapshots.Local
            .FirstOrDefault(s => s.AirlineCode == airlineCode && s.WindowMinutes == window);

        if (snapshot is not null) return snapshot;

        snapshot = new MetricSnapshot
        {
            Id = Guid.NewGuid(),
            Code = $"MET-{airlineCode}-{window}m",
            AirlineCode = airlineCode,
            WindowMinutes = window,
            CurrencyCode = currencyCode
        };

        _context.MetricSnapshots.Add(snapshot);
        return snapshot;
    }

    private async Task RaiseAlertAsync(MetricSnapshot snapshot, decimal threshold, CancellationToken ct)
    {
        // Check if there's already an active alert for this airline/window
        var existingAlert = await _context.Alerts
            .FirstOrDefaultAsync(a => a.AirlineCode == snapshot.AirlineCode
                && a.WindowMinutes == snapshot.WindowMinutes
                && a.Status == "active", ct);

        if (existingAlert is not null)
        {
            // Update existing alert with latest value
            existingAlert.ActualValue = snapshot.ErrorRate;
            existingAlert.UpdatedAt = DateTime.UtcNow;
            return;
        }

        var alert = new Alert
        {
            Id = Guid.NewGuid(),
            Code = $"ALT-{snapshot.AirlineCode}-{snapshot.WindowMinutes}m-{DateTime.UtcNow:yyyyMMddHHmmss}",
            AirlineCode = snapshot.AirlineCode,
            WindowMinutes = snapshot.WindowMinutes,
            Threshold = threshold,
            ActualValue = snapshot.ErrorRate,
            Status = "active",
            FiredAt = DateTime.UtcNow
        };

        _context.Alerts.Add(alert);

        await _publisher.PublishAsync(NatsSubjects.AlertRaised, new AlertRaisedEvent
        {
            AlertId = alert.Id,
            Code = alert.Code,
            AirlineCode = alert.AirlineCode,
            RuleName = $"error-rate-{alert.WindowMinutes}m",
            WindowMinutes = alert.WindowMinutes,
            Threshold = alert.Threshold,
            ActualValue = alert.ActualValue,
            Status = alert.Status,
            FiredAt = alert.FiredAt
        }, ct);

        _logger.LogWarning("Alert raised: {Airline} error rate {Rate}% exceeds {Threshold}% in {Window}m window",
            alert.AirlineCode, alert.ActualValue, alert.Threshold, alert.WindowMinutes);
    }
}
