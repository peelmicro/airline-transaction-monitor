using Ingestion.Application.Ports;
using Ingestion.Domain.Entities;
using Ingestion.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Ingestion.Infrastructure.Repositories;

/// <summary>
/// EF Core adapter implementing the ITransactionRepository port.
/// </summary>
public class TransactionRepository : ITransactionRepository
{
    private readonly IngestionDbContext _context;

    public TransactionRepository(IngestionDbContext context)
    {
        _context = context;
    }

    public async Task<Transaction> CreateAsync(Transaction transaction, CancellationToken ct = default)
    {
        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync(ct);
        return transaction;
    }

    public async Task<Transaction?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Transactions
            .Include(t => t.Airline)
            .Include(t => t.Acquirer)
            .Include(t => t.CardBrand)
            .Include(t => t.Currency)
            .FirstOrDefaultAsync(t => t.Id == id, ct);
    }

    public async Task<(List<Transaction> Items, int TotalCount)> ListAsync(TransactionFilter filter, CancellationToken ct = default)
    {
        var query = _context.Transactions
            .Include(t => t.Airline)
            .Include(t => t.Acquirer)
            .Include(t => t.CardBrand)
            .Include(t => t.Currency)
            .AsQueryable();

        if (!string.IsNullOrEmpty(filter.AirlineCode))
            query = query.Where(t => t.Airline.Code == filter.AirlineCode);

        if (!string.IsNullOrEmpty(filter.AcquirerCode))
            query = query.Where(t => t.Acquirer.Code == filter.AcquirerCode);

        if (!string.IsNullOrEmpty(filter.CardBrandCode))
            query = query.Where(t => t.CardBrand.Code == filter.CardBrandCode);

        if (!string.IsNullOrEmpty(filter.Status))
            query = query.Where(t => t.Status == filter.Status);

        if (filter.FromDate.HasValue)
            query = query.Where(t => t.TransactionDate >= filter.FromDate.Value);

        if (filter.ToDate.HasValue)
            query = query.Where(t => t.TransactionDate <= filter.ToDate.Value);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(t => t.CreatedAt)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }

    public async Task<int> GetNextSequenceAsync(CancellationToken ct = default)
    {
        var count = await _context.Transactions.CountAsync(ct);
        return count + 1;
    }
}
