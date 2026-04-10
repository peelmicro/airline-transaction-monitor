namespace Common.Events;

/// <summary>
/// Published by the Ingestion Service when a new transaction is persisted.
/// Consumed by the Analytics Service (metrics computation) and the Gateway (SignalR push).
/// </summary>
public record TransactionCreatedEvent
{
    public Guid TransactionId { get; init; }
    public string Code { get; init; } = string.Empty;
    public string AirlineCode { get; init; } = string.Empty;
    public string AcquirerCode { get; init; } = string.Empty;
    public string CardBrandCode { get; init; } = string.Empty;
    public string MaskedCard { get; init; } = string.Empty;
    public int Amount { get; init; }
    public string CurrencyCode { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public string FlightNumber { get; init; } = string.Empty;
    public string OriginAirport { get; init; } = string.Empty;
    public string DestinationAirport { get; init; } = string.Empty;
    public string PassengerReference { get; init; } = string.Empty;
    public DateTime TransactionDate { get; init; }
    public DateTime CreatedAt { get; init; }
}
