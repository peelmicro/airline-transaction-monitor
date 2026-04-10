namespace Ingestion.Domain.Entities;

/// <summary>
/// ISO 4217 currency (e.g., EUR, USD, GBP).
/// Referenced by airlines, acquirers, and transactions.
/// </summary>
public class Currency
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string IsoNumber { get; set; } = string.Empty;
    public string Symbol { get; set; } = string.Empty;
    public int DecimalPoints { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    public ICollection<Airline> Airlines { get; set; } = [];
    public ICollection<Acquirer> Acquirers { get; set; } = [];
    public ICollection<Transaction> Transactions { get; set; } = [];
}
