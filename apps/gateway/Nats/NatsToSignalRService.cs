using Common.Events;
using Common.Messaging;
using Gateway.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace Gateway.Nats;

/// <summary>
/// Background service that subscribes to NATS JetStream events and pushes them
/// to connected SignalR clients in real time.
///
/// This is the bridge between the event-driven backend (NATS) and the dashboard (SignalR).
/// Three separate consumers ensure each event type is delivered independently.
/// </summary>
public class NatsToSignalRService : BackgroundService
{
    private readonly IEventSubscriber _subscriber;
    private readonly IHubContext<TransactionHub> _hubContext;
    private readonly ILogger<NatsToSignalRService> _logger;

    public NatsToSignalRService(
        IEventSubscriber subscriber,
        IHubContext<TransactionHub> hubContext,
        ILogger<NatsToSignalRService> logger)
    {
        _subscriber = subscriber;
        _hubContext = hubContext;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Starting NATS-to-SignalR bridge...");

        // Run three consumers in parallel — one per event type
        var tasks = new[]
        {
            SubscribeTransactions(stoppingToken),
            SubscribeMetrics(stoppingToken),
            SubscribeAlerts(stoppingToken)
        };

        await Task.WhenAll(tasks);
    }

    private Task SubscribeTransactions(CancellationToken ct) =>
        _subscriber.SubscribeAsync<TransactionCreatedEvent>(
            NatsSubjects.TransactionsStream,
            NatsSubjects.GatewayTransactionConsumer,
            async (evt, token) =>
            {
                await _hubContext.Clients.All.SendAsync("TransactionCreated", evt, token);
                _logger.LogDebug("Pushed TransactionCreated to SignalR: {Code}", evt.Code);
            },
            ct);

    private Task SubscribeMetrics(CancellationToken ct) =>
        _subscriber.SubscribeAsync<MetricsUpdatedEvent>(
            NatsSubjects.MetricsStream,
            NatsSubjects.GatewayMetricsConsumer,
            async (evt, token) =>
            {
                await _hubContext.Clients.All.SendAsync("MetricsUpdated", evt, token);
                _logger.LogDebug("Pushed MetricsUpdated to SignalR: {Airline}", evt.AirlineCode);
            },
            ct);

    private Task SubscribeAlerts(CancellationToken ct) =>
        _subscriber.SubscribeAsync<AlertRaisedEvent>(
            NatsSubjects.AlertsStream,
            NatsSubjects.GatewayAlertConsumer,
            async (evt, token) =>
            {
                await _hubContext.Clients.All.SendAsync("AlertRaised", evt, token);
                _logger.LogDebug("Pushed AlertRaised to SignalR: {Airline}", evt.AirlineCode);
            },
            ct);
}
