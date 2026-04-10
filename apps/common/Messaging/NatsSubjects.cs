namespace Common.Messaging;

/// <summary>
/// NATS JetStream stream names and subject constants.
/// Centralizes all messaging subjects to avoid magic strings.
/// </summary>
public static class NatsSubjects
{
    // Stream names
    public const string TransactionsStream = "TRANSACTIONS";
    public const string MetricsStream = "METRICS";
    public const string AlertsStream = "ALERTS";

    // Subjects
    public const string TransactionCreated = "transaction.created";
    public const string MetricsUpdated = "metrics.updated";
    public const string AlertRaised = "alert.raised";

    // Durable consumer names
    public const string AnalyticsTransactionConsumer = "analytics-transaction-consumer";
    public const string GatewayTransactionConsumer = "gateway-transaction-consumer";
    public const string GatewayMetricsConsumer = "gateway-metrics-consumer";
    public const string GatewayAlertConsumer = "gateway-alert-consumer";
}
