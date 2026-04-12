using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Common.Messaging;
using NATS.Client.Core;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Moq;

namespace Gateway.IntegrationTests;

public class AuthApiTests : IClassFixture<AuthApiTests.GatewayApiFactory>
{
    private readonly HttpClient _client;

    public AuthApiTests(GatewayApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Login_WithValidCredentials_Returns200AndToken()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            Username = "admin",
            Password = "admin"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(content);
        var root = doc.RootElement;

        Assert.True(root.TryGetProperty("token", out var tokenProp));
        Assert.False(string.IsNullOrWhiteSpace(tokenProp.GetString()));

        Assert.True(root.TryGetProperty("username", out var usernameProp));
        Assert.Equal("admin", usernameProp.GetString());

        Assert.True(root.TryGetProperty("expiresAt", out _));
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_Returns401()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            Username = "admin",
            Password = "wrongpassword"
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task HealthEndpoint_Returns200()
    {
        var response = await _client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    public class GatewayApiFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
                // Remove all NATS-related services and replace with mocks
                services.RemoveAll<INatsConnection>();
                services.RemoveAll<IEventPublisher>();
                services.RemoveAll<IEventSubscriber>();

                // Use a mock INatsConnection - the NatsStreamConfigurator will detect
                // it's not a NatsConnection instance and skip stream configuration
                services.AddSingleton(new Mock<INatsConnection>().Object);
                services.AddSingleton(new Mock<IEventPublisher>().Object);
                services.AddSingleton(new Mock<IEventSubscriber>().Object);

                // Remove all hosted services (NatsToSignalR, etc.)
                services.RemoveAll<IHostedService>();

                // Remove all health check registrations to avoid NATS check failing
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
