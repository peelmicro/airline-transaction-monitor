namespace Analytics.Domain.Entities;

/// <summary>
/// Alert raised when an airline's error rate exceeds the configured threshold.
/// Persisted for audit purposes.
/// </summary>
public class Alert
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string AirlineCode { get; set; } = string.Empty;
    public int WindowMinutes { get; set; }
    public decimal Threshold { get; set; }
    public decimal ActualValue { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime FiredAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
