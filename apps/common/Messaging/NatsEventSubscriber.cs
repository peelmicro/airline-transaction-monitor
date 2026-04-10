using Microsoft.Extensions.Logging;
using NATS.Client.Core;
using NATS.Client.JetStream;
using NATS.Client.JetStream.Models;

namespace Common.Messaging;

/// <summary>
/// NATS JetStream adapter implementing IEventSubscriber.
/// Consumes messages from durable JetStream consumers with automatic acknowledgment.
/// </summary>
public class NatsEventSubscriber : IEventSubscriber
{
    private readonly INatsConnection _connection;
    private readonly ILogger<NatsEventSubscriber> _logger;

    public NatsEventSubscriber(INatsConnection connection, ILogger<NatsEventSubscriber> logger)
    {
        _connection = connection;
        _logger = logger;
    }

    public async Task SubscribeAsync<T>(string stream, string consumer, Func<T, CancellationToken, Task> handler, CancellationToken cancellationToken = default)
    {
        var js = new NatsJSContext((NatsConnection)_connection);

        // Get or create the consumer
        var consumerConfig = new ConsumerConfig(consumer)
        {
            DurableName = consumer,
            AckPolicy = ConsumerConfigAckPolicy.Explicit
        };

        var jsConsumer = await js.CreateOrUpdateConsumerAsync(stream, consumerConfig, cancellationToken);

        _logger.LogInformation("Subscribed to stream {Stream} with consumer {Consumer}", stream, consumer);

        // Consume messages in a loop
        await foreach (var msg in jsConsumer.ConsumeAsync<T>(cancellationToken: cancellationToken))
        {
            try
            {
                if (msg.Data is not null)
                {
                    await handler(msg.Data, cancellationToken);
                }
                await msg.AckAsync(cancellationToken: cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing message from {Stream}/{Consumer}", stream, consumer);
                // NAK so the message is redelivered
                await msg.NakAsync(cancellationToken: cancellationToken);
            }
        }
    }
}
