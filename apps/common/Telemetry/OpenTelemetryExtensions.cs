using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Common.Telemetry;

/// <summary>
/// Extension methods to configure OpenTelemetry consistently across all services.
/// Exports traces to Jaeger (OTLP) and metrics to Prometheus.
/// </summary>
public static class OpenTelemetryExtensions
{
    public static IServiceCollection AddObservability(this IServiceCollection services, string serviceName, string otlpEndpoint = "http://localhost:4317")
    {
        services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService(serviceName))
            .WithTracing(tracing =>
            {
                tracing
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddEntityFrameworkCoreInstrumentation()
                    .AddSource(serviceName)
                    .AddOtlpExporter(options =>
                    {
                        options.Endpoint = new Uri(otlpEndpoint);
                    });
            })
            .WithMetrics(metrics =>
            {
                metrics
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddMeter("Npgsql")
                    .AddMeter("NATS.Client")
                    .AddPrometheusExporter();
            });

        return services;
    }
}
