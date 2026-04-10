using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Simulator.Generators;

namespace Simulator.Controllers;

[ApiController]
[Route("api/simulator")]
public class SimulatorController : ControllerBase
{
    private readonly TransactionGenerator _generator;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<SimulatorController> _logger;
    private static string? _cachedToken;
    private static DateTime _tokenExpiry = DateTime.MinValue;

    public SimulatorController(
        TransactionGenerator generator,
        IHttpClientFactory httpClientFactory,
        ILogger<SimulatorController> logger)
    {
        _generator = generator;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    /// <summary>
    /// Generate transactions for an airline and forward them to the Gateway.
    /// </summary>
    [HttpPost("generate")]
    public async Task<IActionResult> Generate([FromBody] GenerateRequest request, CancellationToken ct)
    {
        var transactions = _generator.Generate(request.AirlineCode, request.Count, request.ErrorRate);
        var results = await ForwardTransactionsAsync(transactions, ct);

        return Ok(new
        {
            airlineCode = request.AirlineCode,
            requested = request.Count,
            sent = results.Succeeded,
            failed = results.Failed,
            errorRate = request.ErrorRate
        });
    }

    /// <summary>
    /// Burst mode: same as generate but intended for high error rate demo scenarios.
    /// </summary>
    [HttpPost("burst")]
    public async Task<IActionResult> Burst([FromBody] GenerateRequest request, CancellationToken ct)
    {
        var transactions = _generator.Generate(request.AirlineCode, request.Count, request.ErrorRate);
        var results = await ForwardTransactionsAsync(transactions, ct);

        _logger.LogWarning("Burst sent: {Count} transactions for {Airline} with {ErrorRate}% error rate",
            request.Count, request.AirlineCode, request.ErrorRate * 100);

        return Ok(new
        {
            mode = "burst",
            airlineCode = request.AirlineCode,
            requested = request.Count,
            sent = results.Succeeded,
            failed = results.Failed,
            errorRate = request.ErrorRate
        });
    }

    /// <summary>
    /// Health check for the simulator.
    /// </summary>
    [HttpGet("health")]
    public IActionResult Health() => Ok(new { status = "healthy", service = "transaction-simulator" });

    private async Task<(int Succeeded, int Failed)> ForwardTransactionsAsync(
        List<GeneratedTransaction> transactions, CancellationToken ct)
    {
        var client = _httpClientFactory.CreateClient("gateway");
        var token = await GetTokenAsync(ct);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        int succeeded = 0, failed = 0;

        foreach (var txn in transactions)
        {
            try
            {
                var json = JsonSerializer.Serialize(txn, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await client.PostAsync("/api/transactions", content, ct);

                if (response.IsSuccessStatusCode) succeeded++;
                else
                {
                    failed++;
                    _logger.LogWarning("Failed to forward transaction: {StatusCode}", response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                failed++;
                _logger.LogError(ex, "Error forwarding transaction");
            }
        }

        return (succeeded, failed);
    }

    private async Task<string> GetTokenAsync(CancellationToken ct)
    {
        if (_cachedToken is not null && DateTime.UtcNow < _tokenExpiry)
            return _cachedToken;

        var client = _httpClientFactory.CreateClient("gateway");
        var loginJson = JsonSerializer.Serialize(new { username = "simulator", password = "simulator" });
        var content = new StringContent(loginJson, Encoding.UTF8, "application/json");
        var response = await client.PostAsync("/api/auth/login", content, ct);
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadAsStringAsync(ct);
        var loginResponse = JsonSerializer.Deserialize<JsonElement>(body);

        _cachedToken = loginResponse.GetProperty("token").GetString()!;
        _tokenExpiry = DateTime.UtcNow.AddMinutes(50); // Refresh before actual expiry

        _logger.LogInformation("Simulator acquired JWT token");
        return _cachedToken;
    }
}

public record GenerateRequest
{
    public string AirlineCode { get; init; } = string.Empty;
    public int Count { get; init; } = 5;
    public double ErrorRate { get; init; } = 0.05;
}
