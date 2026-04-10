namespace Ingestion.Domain.Entities;

/// <summary>
/// Airline retail transaction (in-flight sale, ancillary service, ticket upgrade).
/// Amount stored in minor units (cents) to avoid floating-point issues.
/// </summary>
public class Transaction
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string MaskedCard { get; set; } = string.Empty;
    public Guid AirlineId { get; set; }
    public Guid AcquirerId { get; set; }
    public Guid CardBrandId { get; set; }
    public int Amount { get; set; }
    public Guid CurrencyId { get; set; }
    public DateTime TransactionDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string FlightNumber { get; set; } = string.Empty;
    public string OriginAirport { get; set; } = string.Empty;
    public string DestinationAirport { get; set; } = string.Empty;
    public string PassengerReference { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    public Airline Airline { get; set; } = null!;
    public Acquirer Acquirer { get; set; } = null!;
    public CardBrand CardBrand { get; set; } = null!;
    public Currency Currency { get; set; } = null!;
}
