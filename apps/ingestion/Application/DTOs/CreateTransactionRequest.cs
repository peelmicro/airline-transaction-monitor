using System.ComponentModel.DataAnnotations;

namespace Ingestion.Application.DTOs;

public class CreateTransactionRequest
{
    [Required, MaxLength(20)]
    public string AirlineCode { get; set; } = string.Empty;

    [Required, MaxLength(19)]
    public string MaskedCard { get; set; } = string.Empty;

    [Required, MaxLength(20)]
    public string CardBrandCode { get; set; } = string.Empty;

    [Required]
    public int Amount { get; set; }

    [Required, MaxLength(3)]
    public string CurrencyCode { get; set; } = string.Empty;

    [Required, MaxLength(20)]
    public string AcquirerCode { get; set; } = string.Empty;

    [Required, MaxLength(20)]
    public string Status { get; set; } = string.Empty;

    public DateTime TransactionDate { get; set; } = DateTime.UtcNow;

    [MaxLength(20)]
    public string FlightNumber { get; set; } = string.Empty;

    [MaxLength(3)]
    public string OriginAirport { get; set; } = string.Empty;

    [MaxLength(3)]
    public string DestinationAirport { get; set; } = string.Empty;

    [MaxLength(100)]
    public string PassengerReference { get; set; } = string.Empty;
}
