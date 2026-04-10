namespace Common.Events;

/// <summary>
/// Published by the Analytics Service when per-airline metrics are recalculated.
/// Consumed by the Gateway (SignalR push to dashboard).
/// </summary>
public record MetricsUpdatedEvent
{
    public string AirlineCode { get; init; } = string.Empty;
    public int WindowMinutes { get; init; }
    public int TransactionCount { get; init; }
    public int TotalVolume { get; init; }
    public string CurrencyCode { get; init; } = string.Empty;
    public int ErrorCount { get; init; }
    public decimal ErrorRate { get; init; }
    public int LatencyP95Ms { get; init; }
    public int LatencyP99Ms { get; init; }
    public DateTime Timestamp { get; init; }
}
