using Common.Auth;
using Common.Messaging;
using Common.Telemetry;
using Ingestion.Application.Ports;
using Ingestion.Infrastructure.Data;
using Ingestion.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Database
builder.Services.AddDbContext<IngestionDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("IngestionDb")));

// Repository (hexagonal adapter)
builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();

// NATS messaging
builder.Services.AddNatsMessaging(builder.Configuration["Nats:Url"] ?? "nats://localhost:4222");

// Authentication
builder.Services.AddJwtAuthentication(builder.Configuration);

// Controllers
builder.Services.AddControllers();

// Observability
builder.Services.AddObservability("ingestion-service",
    builder.Configuration["Otlp:Endpoint"] ?? "http://localhost:4317");

// OpenAPI / Swagger
builder.Services.AddOpenApi();

// Health checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<IngestionDbContext>()
    .AddCheck<NatsHealthCheck>("nats");

var app = builder.Build();

// Auto-migrate and seed on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<IngestionDbContext>();
    await IngestionDbSeeder.SeedAsync(db);
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAuthentication();
app.UseAuthorization();

// Prometheus metrics endpoint
app.UseOpenTelemetryPrometheusScrapingEndpoint();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();

// Needed for WebApplicationFactory in integration tests
public partial class Program { }
