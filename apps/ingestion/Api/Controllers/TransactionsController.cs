using Common.Events;
using Common.Messaging;
using Ingestion.Application.DTOs;
using Ingestion.Application.Ports;
using Ingestion.Domain.Entities;
using Ingestion.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ingestion.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/transactions")]
public class TransactionsController : ControllerBase
{
    private readonly ITransactionRepository _repository;
    private readonly IEventPublisher _publisher;
    private readonly IngestionDbContext _context;
    private readonly ILogger<TransactionsController> _logger;

    public TransactionsController(
        ITransactionRepository repository,
        IEventPublisher publisher,
        IngestionDbContext context,
        ILogger<TransactionsController> logger)
    {
        _repository = repository;
        _publisher = publisher;
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Create a new transaction. Validates, persists, and publishes transaction.created event.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTransactionRequest request, CancellationToken ct)
    {
        // Resolve foreign keys from codes
        var airline = await _context.Airlines.FirstOrDefaultAsync(a => a.Code == request.AirlineCode, ct);
        if (airline is null) return BadRequest(new { message = $"Unknown airline: {request.AirlineCode}" });

        var acquirer = await _context.Acquirers.FirstOrDefaultAsync(a => a.Code == request.AcquirerCode, ct);
        if (acquirer is null) return BadRequest(new { message = $"Unknown acquirer: {request.AcquirerCode}" });

        var cardBrand = await _context.CardBrands.FirstOrDefaultAsync(cb => cb.Code == request.CardBrandCode, ct);
        if (cardBrand is null) return BadRequest(new { message = $"Unknown card brand: {request.CardBrandCode}" });

        var currency = await _context.Currencies.FirstOrDefaultAsync(c => c.Code == request.CurrencyCode, ct);
        if (currency is null) return BadRequest(new { message = $"Unknown currency: {request.CurrencyCode}" });

        // Generate sequential code
        var seq = await _repository.GetNextSequenceAsync(ct);
        var code = $"TXN-{DateTime.UtcNow:yyyy-MM}-{seq:D6}";

        var transaction = new Transaction
        {
            Id = Guid.NewGuid(),
            Code = code,
            MaskedCard = request.MaskedCard,
            AirlineId = airline.Id,
            AcquirerId = acquirer.Id,
            CardBrandId = cardBrand.Id,
            Amount = request.Amount,
            CurrencyId = currency.Id,
            TransactionDate = request.TransactionDate,
            Status = request.Status,
            FlightNumber = request.FlightNumber,
            OriginAirport = request.OriginAirport,
            DestinationAirport = request.DestinationAirport,
            PassengerReference = request.PassengerReference
        };

        await _repository.CreateAsync(transaction, ct);

        // Publish event to NATS
        var evt = new TransactionCreatedEvent
        {
            TransactionId = transaction.Id,
            Code = transaction.Code,
            AirlineCode = request.AirlineCode,
            AcquirerCode = request.AcquirerCode,
            CardBrandCode = request.CardBrandCode,
            MaskedCard = transaction.MaskedCard,
            Amount = transaction.Amount,
            CurrencyCode = request.CurrencyCode,
            Status = transaction.Status,
            FlightNumber = transaction.FlightNumber,
            OriginAirport = transaction.OriginAirport,
            DestinationAirport = transaction.DestinationAirport,
            PassengerReference = transaction.PassengerReference,
            TransactionDate = transaction.TransactionDate,
            CreatedAt = transaction.CreatedAt
        };

        await _publisher.PublishAsync(NatsSubjects.TransactionCreated, evt, ct);
        _logger.LogInformation("Transaction created: {Code} for {Airline}", code, request.AirlineCode);

        return CreatedAtAction(nameof(GetById), new { id = transaction.Id }, MapToResponse(transaction, request));
    }

    /// <summary>
    /// List transactions with optional filters.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> List([FromQuery] TransactionFilter filter, CancellationToken ct)
    {
        var (items, totalCount) = await _repository.ListAsync(filter, ct);

        var response = new
        {
            items = items.Select(t => new TransactionResponse
            {
                Id = t.Id,
                Code = t.Code,
                MaskedCard = t.MaskedCard,
                AirlineCode = t.Airline.Code,
                AcquirerCode = t.Acquirer.Code,
                CardBrandCode = t.CardBrand.Code,
                Amount = t.Amount,
                CurrencyCode = t.Currency.Code,
                Status = t.Status,
                TransactionDate = t.TransactionDate,
                FlightNumber = t.FlightNumber,
                OriginAirport = t.OriginAirport,
                DestinationAirport = t.DestinationAirport,
                PassengerReference = t.PassengerReference,
                CreatedAt = t.CreatedAt
            }),
            totalCount,
            page = filter.Page,
            pageSize = filter.PageSize
        };

        return Ok(response);
    }

    /// <summary>
    /// Get a single transaction by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var transaction = await _repository.GetByIdAsync(id, ct);
        if (transaction is null) return NotFound();

        var response = new TransactionResponse
        {
            Id = transaction.Id,
            Code = transaction.Code,
            MaskedCard = transaction.MaskedCard,
            AirlineCode = transaction.Airline.Code,
            AcquirerCode = transaction.Acquirer.Code,
            CardBrandCode = transaction.CardBrand.Code,
            Amount = transaction.Amount,
            CurrencyCode = transaction.Currency.Code,
            Status = transaction.Status,
            TransactionDate = transaction.TransactionDate,
            FlightNumber = transaction.FlightNumber,
            OriginAirport = transaction.OriginAirport,
            DestinationAirport = transaction.DestinationAirport,
            PassengerReference = transaction.PassengerReference,
            CreatedAt = transaction.CreatedAt
        };

        return Ok(response);
    }

    private static TransactionResponse MapToResponse(Transaction t, CreateTransactionRequest req) => new()
    {
        Id = t.Id,
        Code = t.Code,
        MaskedCard = t.MaskedCard,
        AirlineCode = req.AirlineCode,
        AcquirerCode = req.AcquirerCode,
        CardBrandCode = req.CardBrandCode,
        Amount = t.Amount,
        CurrencyCode = req.CurrencyCode,
        Status = t.Status,
        TransactionDate = t.TransactionDate,
        FlightNumber = t.FlightNumber,
        OriginAirport = t.OriginAirport,
        DestinationAirport = t.DestinationAirport,
        PassengerReference = t.PassengerReference,
        CreatedAt = t.CreatedAt
    };
}
