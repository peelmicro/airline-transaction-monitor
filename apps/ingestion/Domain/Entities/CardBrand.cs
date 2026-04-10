namespace Ingestion.Domain.Entities;

/// <summary>
/// Card network brand (e.g., Visa, Mastercard, Amex).
/// </summary>
public class CardBrand
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public DateTime? DisabledAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navigation properties
    public ICollection<Transaction> Transactions { get; set; } = [];
}
