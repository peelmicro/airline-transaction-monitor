namespace Common.Messaging;

/// <summary>
/// Port for publishing events to the message broker.
/// Services depend on this interface — the NATS adapter implements it.
/// This keeps the domain/application layer free of NATS dependencies.
/// </summary>
public interface IEventPublisher
{
    /// <summary>
    /// Publishes an event to the specified subject.
    /// </summary>
    /// <typeparam name="T">Event type (e.g., TransactionCreatedEvent)</typeparam>
    /// <param name="subject">NATS subject (e.g., "transaction.created")</param>
    /// <param name="data">Event payload</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task PublishAsync<T>(string subject, T data, CancellationToken cancellationToken = default);
}
