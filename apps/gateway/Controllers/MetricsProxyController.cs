using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gateway.Controllers;

/// <summary>
/// Proxies metrics and alerts requests to the Analytics Service.
/// </summary>
[ApiController]
[Authorize]
public class MetricsProxyController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;

    public MetricsProxyController(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    /// <summary>
    /// Get metrics for a specific airline (proxied to Analytics Service).
    /// </summary>
    [HttpGet("api/airlines/{code}/metrics")]
    public async Task<IActionResult> GetAirlineMetrics(string code)
    {
        var client = CreateAuthenticatedClient("analytics");
        var response = await client.GetAsync($"/api/airlines/{code}/metrics");
        var body = await response.Content.ReadAsStringAsync();

        return StatusCode((int)response.StatusCode, body);
    }

    /// <summary>
    /// Get alerts with optional filters (proxied to Analytics Service).
    /// </summary>
    [HttpGet("api/alerts")]
    public async Task<IActionResult> GetAlerts()
    {
        var client = CreateAuthenticatedClient("analytics");
        var queryString = Request.QueryString.Value ?? "";
        var response = await client.GetAsync($"/api/alerts{queryString}");
        var body = await response.Content.ReadAsStringAsync();

        return StatusCode((int)response.StatusCode, body);
    }

    private HttpClient CreateAuthenticatedClient(string name)
    {
        var client = _httpClientFactory.CreateClient(name);
        var authHeader = Request.Headers.Authorization.ToString();
        if (!string.IsNullOrEmpty(authHeader))
            client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", authHeader);
        return client;
    }
}
