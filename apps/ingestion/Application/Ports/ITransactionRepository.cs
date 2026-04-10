using Ingestion.Domain.Entities;

namespace Ingestion.Application.Ports;

/// <summary>
/// Port for transaction persistence. Implemented by the EF Core adapter.
/// </summary>
public interface ITransactionRepository
{
    Task<Transaction> CreateAsync(Transaction transaction, CancellationToken ct = default);
    Task<Transaction?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<(List<Transaction> Items, int TotalCount)> ListAsync(TransactionFilter filter, CancellationToken ct = default);
    Task<int> GetNextSequenceAsync(CancellationToken ct = default);
}

public record TransactionFilter
{
    public string? AirlineCode { get; init; }
    public string? AcquirerCode { get; init; }
    public string? CardBrandCode { get; init; }
    public string? Status { get; init; }
    public DateTime? FromDate { get; init; }
    public DateTime? ToDate { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 50;
}
