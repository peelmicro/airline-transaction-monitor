using Common.Events;
using Common.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Analytics.Application.Services;

/// <summary>
/// Background service that consumes transaction.created events from NATS
/// and delegates processing to the MetricsService.
/// </summary>
public class TransactionConsumerService : BackgroundService
{
    private readonly IEventSubscriber _subscriber;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<TransactionConsumerService> _logger;

    public TransactionConsumerService(
        IEventSubscriber subscriber,
        IServiceScopeFactory scopeFactory,
        ILogger<TransactionConsumerService> logger)
    {
        _subscriber = subscriber;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Starting transaction consumer...");

        await _subscriber.SubscribeAsync<TransactionCreatedEvent>(
            NatsSubjects.TransactionsStream,
            NatsSubjects.AnalyticsTransactionConsumer,
            async (evt, ct) =>
            {
                // Create a new scope per message (MetricsService depends on scoped DbContext)
                using var scope = _scopeFactory.CreateScope();
                var metricsService = scope.ServiceProvider.GetRequiredService<MetricsService>();
                await metricsService.ProcessTransactionAsync(evt, ct);
            },
            stoppingToken);
    }
}
