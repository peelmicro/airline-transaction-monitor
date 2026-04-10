namespace Common.Events;

/// <summary>
/// Published by the Analytics Service when an alert rule is breached.
/// Consumed by the Gateway (SignalR push to dashboard).
/// </summary>
public record AlertRaisedEvent
{
    public Guid AlertId { get; init; }
    public string Code { get; init; } = string.Empty;
    public string AirlineCode { get; init; } = string.Empty;
    public string RuleName { get; init; } = string.Empty;
    public int WindowMinutes { get; init; }
    public decimal Threshold { get; init; }
    public decimal ActualValue { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTime FiredAt { get; init; }
}
