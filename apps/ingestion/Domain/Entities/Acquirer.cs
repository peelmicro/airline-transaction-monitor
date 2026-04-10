namespace Ingestion.Domain.Entities;

/// <summary>
/// Payment acquirer (e.g., Adyen, Worldpay, Elavon).
/// Processes card payments on behalf of airlines.
/// </summary>
public class Acquirer
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public Guid CurrencyId { get; set; }
    public DateTime? DisabledAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    public Currency Currency { get; set; } = null!;
    public ICollection<Transaction> Transactions { get; set; } = [];
}
