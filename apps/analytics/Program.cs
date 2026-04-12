using Common.Auth;
using Common.Messaging;
using Common.Telemetry;
using Analytics.Application.Services;
using Analytics.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Database
builder.Services.AddDbContext<AnalyticsDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("AnalyticsDb")));

// NATS messaging
builder.Services.AddNatsMessaging(builder.Configuration["Nats:Url"] ?? "nats://localhost:4222");

// Application services
builder.Services.AddScoped<MetricsService>();
builder.Services.AddHostedService<TransactionConsumerService>();

// Authentication
builder.Services.AddJwtAuthentication(builder.Configuration);

// Controllers
builder.Services.AddControllers();

// Observability
builder.Services.AddObservability("analytics-service",
    builder.Configuration["Otlp:Endpoint"] ?? "http://localhost:4317");

// OpenAPI / Swagger
builder.Services.AddOpenApi();

// Health checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<AnalyticsDbContext>()
    .AddCheck<NatsHealthCheck>("nats");

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

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();

// Needed for WebApplicationFactory in integration tests
public partial class Program { }
