using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.EntityFrameworkCore;
using Analytics.Infrastructure.Data;
using Common.Messaging;
using NATS.Client.Core;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Moq;

namespace Analytics.IntegrationTests;

public class AlertsApiTests : IClassFixture<AlertsApiTests.AnalyticsApiFactory>
{
    private readonly HttpClient _client;

    public AlertsApiTests(AnalyticsApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetAlerts_WithoutAuth_Returns401()
    {
        var response = await _client.GetAsync("/api/alerts");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task HealthEndpoint_Returns200()
    {
        var response = await _client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    public class AnalyticsApiFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
                // Remove all EF Core / DbContext registrations to avoid dual-provider conflict
                var descriptorsToRemove = services
                    .Where(d => d.ServiceType == typeof(DbContextOptions<AnalyticsDbContext>)
                        || d.ServiceType == typeof(AnalyticsDbContext)
                        || d.ServiceType.FullName?.Contains("EntityFrameworkCore") == true
                        || d.ServiceType.FullName?.Contains("DbContextOptions") == true)
                    .ToList();
                foreach (var d in descriptorsToRemove)
                    services.Remove(d);

                // Add InMemory database
                var dbName = "AnalyticsTestDb_" + Guid.NewGuid();
                services.AddDbContext<AnalyticsDbContext>(options =>
                    options.UseInMemoryDatabase(dbName));

                // Remove NATS-related services and replace with mocks
                services.RemoveAll<INatsConnection>();
                services.RemoveAll<IEventPublisher>();
                services.RemoveAll<IEventSubscriber>();

                services.AddSingleton(new Mock<IEventPublisher>().Object);
                services.AddSingleton(new Mock<IEventSubscriber>().Object);
                services.AddSingleton(new Mock<INatsConnection>().Object);

                // Remove the TransactionConsumerService background service
                services.RemoveAll<IHostedService>();

                // Remove all health check registrations to avoid duplicates
                services.RemoveAll<IHealthCheck>();
                var healthCheckDescriptors = services
                    .Where(d => d.ServiceType.FullName?.Contains("HealthCheck") == true)
                    .ToList();
                foreach (var d in healthCheckDescriptors)
                    services.Remove(d);
                services.AddHealthChecks();
            });
        }
    }
}
