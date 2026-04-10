using Common.Auth;
using Common.Telemetry;
using Analytics.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Database
builder.Services.AddDbContext<AnalyticsDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("AnalyticsDb")));

// Authentication
builder.Services.AddJwtAuthentication(builder.Configuration);

// Observability
builder.Services.AddObservability("analytics-service",
    builder.Configuration["Otlp:Endpoint"] ?? "http://localhost:4317");

// OpenAPI / Swagger
builder.Services.AddOpenApi();

// Health checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<AnalyticsDbContext>();

var app = builder.Build();

// Auto-migrate on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AnalyticsDbContext>();
    await AnalyticsDbSeeder.SeedAsync(db);
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
