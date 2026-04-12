using Analytics.Application.Services;
using Analytics.Infrastructure.Data;
using Common.Events;
using Common.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace Analytics.UnitTests;

public class MetricsServiceTests : IDisposable
{
    private readonly AnalyticsDbContext _context;
    private readonly Mock<IEventPublisher> _publisherMock;
    private readonly MetricsService _service;

    public MetricsServiceTests()
    {
        var options = new DbContextOptionsBuilder<AnalyticsDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AnalyticsDbContext(options);
        _publisherMock = new Mock<IEventPublisher>();
        var loggerMock = new Mock<ILogger<MetricsService>>();

        _service = new MetricsService(_context, _publisherMock.Object, loggerMock.Object);
    }

    private TransactionCreatedEvent CreateEvent(
        string airlineCode = "Ryanair",
        string status = "approved",
        int amount = 15000,
        string currencyCode = "EUR")
    {
        return new TransactionCreatedEvent
        {
            TransactionId = Guid.NewGuid(),
            Code = $"TXN-{Guid.NewGuid():N}",
            AirlineCode = airlineCode,
            AcquirerCode = "Adyen",
            CardBrandCode = "Visa",
            MaskedCard = "****-****-****-1234",
            Amount = amount,
            CurrencyCode = currencyCode,
            Status = status,
            FlightNumber = "FR1234",
            OriginAirport = "DUB",
            DestinationAirport = "STN",
            PassengerReference = "PAX-001",
            TransactionDate = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };
    }

    [Fact]
    public async Task ProcessTransactionAsync_CreatesMetricSnapshotsForAllWindows()
    {
        var evt = CreateEvent();

        await _service.ProcessTransactionAsync(evt, CancellationToken.None);

        var snapshots = await _context.MetricSnapshots.ToListAsync();
        Assert.Equal(3, snapshots.Count);

        Assert.Contains(snapshots, s => s.WindowMinutes == 1);
        Assert.Contains(snapshots, s => s.WindowMinutes == 5);
        Assert.Contains(snapshots, s => s.WindowMinutes == 60);
    }

    [Fact]
    public async Task ProcessTransactionAsync_IncrementsTransactionCount()
    {
        var evt = CreateEvent();

        await _service.ProcessTransactionAsync(evt, CancellationToken.None);
        await _service.ProcessTransactionAsync(CreateEvent(), CancellationToken.None);

        var snapshot = await _context.MetricSnapshots
            .FirstOrDefaultAsync(s => s.AirlineCode == "Ryanair" && s.WindowMinutes == 1);

        Assert.NotNull(snapshot);
        Assert.Equal(2, snapshot.TransactionCount);
    }

    [Fact]
    public async Task ProcessTransactionAsync_AccumulatesTotalVolume()
    {
        await _service.ProcessTransactionAsync(CreateEvent(amount: 10000), CancellationToken.None);
        await _service.ProcessTransactionAsync(CreateEvent(amount: 5000), CancellationToken.None);

        var snapshot = await _context.MetricSnapshots
            .FirstOrDefaultAsync(s => s.AirlineCode == "Ryanair" && s.WindowMinutes == 1);

        Assert.NotNull(snapshot);
        Assert.Equal(15000, snapshot.TotalVolume);
    }

    [Fact]
    public async Task ProcessTransactionAsync_CountsErrors()
    {
        await _service.ProcessTransactionAsync(CreateEvent(status: "approved"), CancellationToken.None);
        await _service.ProcessTransactionAsync(CreateEvent(status: "declined"), CancellationToken.None);
        await _service.ProcessTransactionAsync(CreateEvent(status: "failed"), CancellationToken.None);

        var snapshot = await _context.MetricSnapshots
            .FirstOrDefaultAsync(s => s.AirlineCode == "Ryanair" && s.WindowMinutes == 1);

        Assert.NotNull(snapshot);
        Assert.Equal(2, snapshot.ErrorCount);
        Assert.Equal(3, snapshot.TransactionCount);
    }

    [Fact]
    public async Task ProcessTransactionAsync_CalculatesErrorRate()
    {
        await _service.ProcessTransactionAsync(CreateEvent(status: "approved"), CancellationToken.None);
        await _service.ProcessTransactionAsync(CreateEvent(status: "declined"), CancellationToken.None);

        var snapshot = await _context.MetricSnapshots
            .FirstOrDefaultAsync(s => s.AirlineCode == "Ryanair" && s.WindowMinutes == 1);

        Assert.NotNull(snapshot);
        Assert.Equal(50.00m, snapshot.ErrorRate);
    }

    [Fact]
    public async Task ProcessTransactionAsync_PublishesMetricsUpdatedEvents()
    {
        var evt = CreateEvent();

        await _service.ProcessTransactionAsync(evt, CancellationToken.None);

        // Should publish 3 MetricsUpdated events (one per window: 1m, 5m, 60m)
        _publisherMock.Verify(
            p => p.PublishAsync(
                NatsSubjects.MetricsUpdated,
                It.IsAny<MetricsUpdatedEvent>(),
                It.IsAny<CancellationToken>()),
            Times.Exactly(3));
    }

    [Fact]
    public async Task ProcessTransactionAsync_RaisesAlertWhenErrorRateExceedsThreshold()
    {
        // The 1-minute window threshold is 10%. We need error rate > 10%.
        // Send 1 error out of 1 transaction = 100% error rate
        await _service.ProcessTransactionAsync(CreateEvent(status: "declined"), CancellationToken.None);

        // Verify alert was raised (at least one AlertRaised publish for any window)
        _publisherMock.Verify(
            p => p.PublishAsync(
                NatsSubjects.AlertRaised,
                It.IsAny<AlertRaisedEvent>(),
                It.IsAny<CancellationToken>()),
            Times.AtLeastOnce());

        var alerts = await _context.Alerts.ToListAsync();
        Assert.NotEmpty(alerts);
        Assert.All(alerts, a => Assert.Equal("active", a.Status));
    }

    [Fact]
    public async Task ProcessTransactionAsync_DoesNotRaiseAlertWhenErrorRateBelowThreshold()
    {
        // Send 100 approved transactions - error rate = 0%
        for (int i = 0; i < 10; i++)
        {
            await _service.ProcessTransactionAsync(CreateEvent(status: "approved"), CancellationToken.None);
        }

        _publisherMock.Verify(
            p => p.PublishAsync(
                NatsSubjects.AlertRaised,
                It.IsAny<AlertRaisedEvent>(),
                It.IsAny<CancellationToken>()),
            Times.Never());

        var alerts = await _context.Alerts.ToListAsync();
        Assert.Empty(alerts);
    }

    [Fact]
    public async Task ProcessTransactionAsync_SetsCorrectAirlineCodeOnSnapshots()
    {
        await _service.ProcessTransactionAsync(CreateEvent(airlineCode: "Iberia"), CancellationToken.None);
        await _service.ProcessTransactionAsync(CreateEvent(airlineCode: "Ryanair"), CancellationToken.None);

        var iberiaSnapshots = await _context.MetricSnapshots
            .Where(s => s.AirlineCode == "Iberia")
            .ToListAsync();
        var ryanairSnapshots = await _context.MetricSnapshots
            .Where(s => s.AirlineCode == "Ryanair")
            .ToListAsync();

        Assert.Equal(3, iberiaSnapshots.Count);
        Assert.Equal(3, ryanairSnapshots.Count);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
