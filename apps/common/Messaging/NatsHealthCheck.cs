using Microsoft.Extensions.Diagnostics.HealthChecks;
using NATS.Client.Core;

namespace Common.Messaging;

/// <summary>
/// Health check that verifies NATS connectivity.
/// Reports unhealthy if the connection is closed or broken.
/// </summary>
public class NatsHealthCheck : IHealthCheck
{
    private readonly INatsConnection _connection;

    public NatsHealthCheck(INatsConnection connection)
    {
        _connection = connection;
    }

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var result = _connection.ConnectionState == NatsConnectionState.Open
            ? HealthCheckResult.Healthy("NATS connection is open")
            : HealthCheckResult.Unhealthy($"NATS connection state: {_connection.ConnectionState}");

        return Task.FromResult(result);
    }
}
