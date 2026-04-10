using Common.Auth;
using Common.Messaging;
using Common.Telemetry;
using Gateway.Auth;
using Gateway.Hubs;
using Gateway.Nats;
using NATS.Client.Core;

var builder = WebApplication.CreateBuilder(args);

// Authentication
builder.Services.AddJwtAuthentication(builder.Configuration);

// Token service for issuing JWTs
builder.Services.AddSingleton<TokenService>();

// NATS messaging
builder.Services.AddNatsMessaging(builder.Configuration["Nats:Url"] ?? "nats://localhost:4222");

// NATS-to-SignalR background service
builder.Services.AddHostedService<NatsToSignalRService>();

// SignalR for real-time push to dashboard
builder.Services.AddSignalR();

// HTTP clients for proxying to downstream services
builder.Services.AddHttpClient("ingestion", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:IngestionUrl"] ?? "http://localhost:5001");
});
builder.Services.AddHttpClient("analytics", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:AnalyticsUrl"] ?? "http://localhost:5002");
});

// Controllers
builder.Services.AddControllers();

// Observability
builder.Services.AddObservability("gateway-service",
    builder.Configuration["Otlp:Endpoint"] ?? "http://localhost:4317");

// OpenAPI / Swagger
builder.Services.AddOpenApi();

// Health checks
builder.Services.AddHealthChecks()
    .AddCheck<NatsHealthCheck>("nats");

// CORS for Angular dashboard
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

// Configure NATS JetStream streams on startup
using (var scope = app.Services.CreateScope())
{
    var nats = scope.ServiceProvider.GetRequiredService<INatsConnection>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    await NatsStreamConfigurator.ConfigureStreamsAsync(nats, logger);
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

// Prometheus metrics endpoint
app.UseOpenTelemetryPrometheusScrapingEndpoint();

app.MapControllers();
app.MapHub<TransactionHub>("/hub/transactions");
app.MapHealthChecks("/health");

app.Run();
