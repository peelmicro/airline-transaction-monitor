using Microsoft.Extensions.Logging;
using NATS.Client.Core;
using NATS.Client.JetStream;
using NATS.Client.JetStream.Models;

namespace Common.Messaging;

/// <summary>
/// Creates or updates NATS JetStream streams on application startup.
/// Ensures the required streams (TRANSACTIONS, METRICS, ALERTS) exist before services start publishing/subscribing.
/// </summary>
public static class NatsStreamConfigurator
{
    public static async Task ConfigureStreamsAsync(INatsConnection connection, ILogger logger)
    {
        if (connection is not NatsConnection natsConn)
        {
            logger.LogWarning("NATS connection is not a NatsConnection instance; skipping stream configuration");
            return;
        }

        var js = new NatsJSContext(natsConn);

        var streams = new[]
        {
            new StreamConfig(NatsSubjects.TransactionsStream, [NatsSubjects.TransactionCreated]),
            new StreamConfig(NatsSubjects.MetricsStream, [NatsSubjects.MetricsUpdated]),
            new StreamConfig(NatsSubjects.AlertsStream, [NatsSubjects.AlertRaised])
        };

        foreach (var config in streams)
        {
            try
            {
                await js.CreateOrUpdateStreamAsync(config);
                logger.LogInformation("NATS stream '{Stream}' configured with subjects: {Subjects}",
                    config.Name, string.Join(", ", config.Subjects ?? []));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to configure NATS stream '{Stream}'", config.Name);
                throw;
            }
        }
    }
}
