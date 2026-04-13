using Common.Telemetry;
using Simulator.Generators;

var builder = WebApplication.CreateBuilder(args);

// Check if simulator is enabled (can be disabled via SIMULATOR_ENABLED=false)
var simulatorEnabled = builder.Configuration.GetValue("SIMULATOR_ENABLED", true);
if (!simulatorEnabled)
{
    Console.WriteLine("TransactionSimulator is disabled (SIMULATOR_ENABLED=false). Exiting.");
    return;
}

// Transaction generator (Bogus)
builder.Services.AddSingleton<TransactionGenerator>();

// HTTP client for forwarding transactions to the Gateway
builder.Services.AddHttpClient("gateway", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:GatewayUrl"] ?? "http://localhost:5000");
});

// Controllers
builder.Services.AddControllers();

// Observability
builder.Services.AddObservability("simulator-service",
    builder.Configuration["Otlp:Endpoint"] ?? "http://localhost:4317");

// OpenAPI / Swagger
builder.Services.AddOpenApi();

// Health checks
builder.Services.AddHealthChecks();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Prometheus metrics endpoint
app.UseOpenTelemetryPrometheusScrapingEndpoint();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
