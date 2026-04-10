namespace Analytics.Domain.Entities;

/// <summary>
/// Rolling metric snapshot for an airline within a time window (1m, 5m, 1h).
/// Denormalized airline/currency codes to avoid cross-database joins.
/// </summary>
public class MetricSnapshot
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string AirlineCode { get; set; } = string.Empty;
    public int WindowMinutes { get; set; }
    public int TransactionCount { get; set; }
    public int TotalVolume { get; set; }
    public string CurrencyCode { get; set; } = string.Empty;
    public int ErrorCount { get; set; }
    public decimal ErrorRate { get; set; }
    public int LatencyP95Ms { get; set; }
    public int LatencyP99Ms { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
