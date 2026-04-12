using Ingestion.Application.Ports;
using Ingestion.Domain.Entities;
using Ingestion.Infrastructure.Data;
using Ingestion.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Ingestion.UnitTests;

public class TransactionRepositoryTests : IDisposable
{
    private readonly IngestionDbContext _context;
    private readonly TransactionRepository _repository;

    // Shared reference data
    private readonly Currency _currency;
    private readonly Airline _airline;
    private readonly Acquirer _acquirer;
    private readonly CardBrand _cardBrand;

    public TransactionRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<IngestionDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new IngestionDbContext(options);
        _repository = new TransactionRepository(_context);

        // Seed reference data
        _currency = new Currency
        {
            Id = Guid.NewGuid(),
            Code = "USD",
            IsoNumber = "840",
            Symbol = "$",
            DecimalPoints = 2,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _airline = new Airline
        {
            Id = Guid.NewGuid(),
            Code = "Ryanair",
            Name = "Ryanair DAC",
            Country = "IE",
            CurrencyId = _currency.Id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _acquirer = new Acquirer
        {
            Id = Guid.NewGuid(),
            Code = "Adyen",
            Name = "Adyen B.V.",
            Country = "NL",
            CurrencyId = _currency.Id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _cardBrand = new CardBrand
        {
            Id = Guid.NewGuid(),
            Code = "Visa",
            Name = "Visa Inc.",
            Country = "US",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Currencies.Add(_currency);
        _context.Airlines.Add(_airline);
        _context.Acquirers.Add(_acquirer);
        _context.CardBrands.Add(_cardBrand);
        _context.SaveChanges();
    }

    private Transaction CreateTransaction(string code = "TXN-2026-04-000001", string status = "approved", int amount = 15000)
    {
        return new Transaction
        {
            Id = Guid.NewGuid(),
            Code = code,
            MaskedCard = "****-****-****-1234",
            AirlineId = _airline.Id,
            AcquirerId = _acquirer.Id,
            CardBrandId = _cardBrand.Id,
            Amount = amount,
            CurrencyId = _currency.Id,
            TransactionDate = DateTime.UtcNow,
            Status = status,
            FlightNumber = "FR1234",
            OriginAirport = "DUB",
            DestinationAirport = "STN",
            PassengerReference = "PAX-001",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    [Fact]
    public async Task CreateAsync_PersistsTransaction()
    {
        var transaction = CreateTransaction();

        var result = await _repository.CreateAsync(transaction);

        Assert.NotNull(result);
        Assert.Equal(transaction.Id, result.Id);
        Assert.Equal("TXN-2026-04-000001", result.Code);

        var stored = await _context.Transactions.FindAsync(transaction.Id);
        Assert.NotNull(stored);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsTransactionWithNavigationProperties()
    {
        var transaction = CreateTransaction();
        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync();

        var result = await _repository.GetByIdAsync(transaction.Id);

        Assert.NotNull(result);
        Assert.Equal(transaction.Id, result.Id);
        Assert.Equal("Ryanair", result.Airline.Code);
        Assert.Equal("Adyen", result.Acquirer.Code);
        Assert.Equal("Visa", result.CardBrand.Code);
        Assert.Equal("USD", result.Currency.Code);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNullForNonExistentId()
    {
        var result = await _repository.GetByIdAsync(Guid.NewGuid());

        Assert.Null(result);
    }

    [Fact]
    public async Task ListAsync_ReturnsAllTransactions()
    {
        _context.Transactions.Add(CreateTransaction("TXN-001"));
        _context.Transactions.Add(CreateTransaction("TXN-002"));
        _context.Transactions.Add(CreateTransaction("TXN-003"));
        await _context.SaveChangesAsync();

        var filter = new TransactionFilter { Page = 1, PageSize = 50 };
        var (items, totalCount) = await _repository.ListAsync(filter);

        Assert.Equal(3, totalCount);
        Assert.Equal(3, items.Count);
    }

    [Fact]
    public async Task ListAsync_FiltersByAirlineCode()
    {
        var otherAirline = new Airline
        {
            Id = Guid.NewGuid(),
            Code = "Iberia",
            Name = "Iberia",
            Country = "ES",
            CurrencyId = _currency.Id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        _context.Airlines.Add(otherAirline);

        _context.Transactions.Add(CreateTransaction("TXN-001"));
        var txn2 = CreateTransaction("TXN-002");
        txn2.AirlineId = otherAirline.Id;
        _context.Transactions.Add(txn2);
        await _context.SaveChangesAsync();

        var filter = new TransactionFilter { AirlineCode = "Ryanair", Page = 1, PageSize = 50 };
        var (items, totalCount) = await _repository.ListAsync(filter);

        Assert.Equal(1, totalCount);
        Assert.All(items, t => Assert.Equal("Ryanair", t.Airline.Code));
    }

    [Fact]
    public async Task ListAsync_FiltersByStatus()
    {
        _context.Transactions.Add(CreateTransaction("TXN-001", "approved"));
        _context.Transactions.Add(CreateTransaction("TXN-002", "declined"));
        _context.Transactions.Add(CreateTransaction("TXN-003", "approved"));
        await _context.SaveChangesAsync();

        var filter = new TransactionFilter { Status = "declined", Page = 1, PageSize = 50 };
        var (items, totalCount) = await _repository.ListAsync(filter);

        Assert.Equal(1, totalCount);
        Assert.All(items, t => Assert.Equal("declined", t.Status));
    }

    [Fact]
    public async Task ListAsync_PaginatesCorrectly()
    {
        for (int i = 1; i <= 5; i++)
            _context.Transactions.Add(CreateTransaction($"TXN-{i:D3}"));
        await _context.SaveChangesAsync();

        var filter = new TransactionFilter { Page = 2, PageSize = 2 };
        var (items, totalCount) = await _repository.ListAsync(filter);

        Assert.Equal(5, totalCount);
        Assert.Equal(2, items.Count);
    }

    [Fact]
    public async Task GetNextSequenceAsync_ReturnsCountPlusOne()
    {
        _context.Transactions.Add(CreateTransaction("TXN-001"));
        _context.Transactions.Add(CreateTransaction("TXN-002"));
        await _context.SaveChangesAsync();

        var seq = await _repository.GetNextSequenceAsync();

        Assert.Equal(3, seq);
    }

    [Fact]
    public async Task GetNextSequenceAsync_ReturnsOneWhenEmpty()
    {
        var seq = await _repository.GetNextSequenceAsync();

        Assert.Equal(1, seq);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
