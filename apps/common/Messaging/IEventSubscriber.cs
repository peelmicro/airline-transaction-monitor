namespace Common.Messaging;

/// <summary>
/// Port for subscribing to events from the message broker.
/// Services depend on this interface — the NATS adapter implements it.
/// </summary>
public interface IEventSubscriber
{
    /// <summary>
    /// Subscribes to a JetStream stream and processes messages with the provided handler.
    /// Uses a durable consumer so messages are not lost if the service restarts.
    /// </summary>
    /// <typeparam name="T">Event type to deserialize</typeparam>
    /// <param name="stream">JetStream stream name (e.g., "TRANSACTIONS")</param>
    /// <param name="consumer">Durable consumer name (e.g., "analytics-transaction-consumer")</param>
    /// <param name="handler">Async handler called for each message</param>
    /// <param name="cancellationToken">Cancellation token to stop consuming</param>
    Task SubscribeAsync<T>(string stream, string consumer, Func<T, CancellationToken, Task> handler, CancellationToken cancellationToken = default);
}
