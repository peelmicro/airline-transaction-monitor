namespace Ingestion.Application.DTOs;

public record TransactionResponse
{
    public Guid Id { get; init; }
    public string Code { get; init; } = string.Empty;
    public string MaskedCard { get; init; } = string.Empty;
    public string AirlineCode { get; init; } = string.Empty;
    public string AcquirerCode { get; init; } = string.Empty;
    public string CardBrandCode { get; init; } = string.Empty;
    public int Amount { get; init; }
    public string CurrencyCode { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTime TransactionDate { get; init; }
    public string FlightNumber { get; init; } = string.Empty;
    public string OriginAirport { get; init; } = string.Empty;
    public string DestinationAirport { get; init; } = string.Empty;
    public string PassengerReference { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
}
