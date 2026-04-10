using Microsoft.Extensions.Logging;
using NATS.Client.Core;
using NATS.Client.JetStream;

namespace Common.Messaging;

/// <summary>
/// NATS JetStream adapter implementing IEventPublisher.
/// Publishes events to JetStream subjects with JSON serialization.
/// </summary>
public class NatsEventPublisher : IEventPublisher
{
    private readonly INatsConnection _connection;
    private readonly ILogger<NatsEventPublisher> _logger;

    public NatsEventPublisher(INatsConnection connection, ILogger<NatsEventPublisher> logger)
    {
        _connection = connection;
        _logger = logger;
    }

    public async Task PublishAsync<T>(string subject, T data, CancellationToken cancellationToken = default)
    {
        var js = new NatsJSContext((NatsConnection)_connection);
        await js.PublishAsync(subject, data, cancellationToken: cancellationToken);
        _logger.LogDebug("Published event to {Subject}", subject);
    }
}
