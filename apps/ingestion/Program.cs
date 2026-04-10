using Common.Auth;
using Common.Telemetry;
using Ingestion.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Database
builder.Services.AddDbContext<IngestionDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("IngestionDb")));

// Authentication
builder.Services.AddJwtAuthentication(builder.Configuration);

// Observability
builder.Services.AddObservability("ingestion-service",
    builder.Configuration["Otlp:Endpoint"] ?? "http://localhost:4317");

// OpenAPI / Swagger
builder.Services.AddOpenApi();

// Health checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<IngestionDbContext>();

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

app.MapHealthChecks("/health");

app.Run();
