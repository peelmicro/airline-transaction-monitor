namespace Ingestion.Domain.Entities;

/// <summary>
/// Airline client (e.g., Ryanair, Iberia, British Airways).
/// Each airline has a default operating currency.
/// </summary>
public class Airline
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
