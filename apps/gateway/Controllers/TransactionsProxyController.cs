using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gateway.Controllers;

/// <summary>
/// Proxies transaction requests to the Ingestion Service.
/// In production, this would use YARP or a dedicated API gateway (Kong, Envoy).
/// </summary>
[ApiController]
[Route("api/transactions")]
[Authorize]
public class TransactionsProxyController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<TransactionsProxyController> _logger;

    public TransactionsProxyController(IHttpClientFactory httpClientFactory, ILogger<TransactionsProxyController> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    /// <summary>
    /// Create a new transaction (proxied to Ingestion Service).
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create()
    {
        var client = CreateAuthenticatedClient("ingestion");
        var content = new StreamContent(Request.Body);
        content.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse(
            Request.ContentType ?? "application/json");

        var response = await client.PostAsync("/api/transactions", content);
        var body = await response.Content.ReadAsStringAsync();

        return StatusCode((int)response.StatusCode, body);
    }

    /// <summary>
    /// List transactions with optional filters (proxied to Ingestion Service).
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> List()
    {
        var client = CreateAuthenticatedClient("ingestion");
        var queryString = Request.QueryString.Value ?? "";
        var response = await client.GetAsync($"/api/transactions{queryString}");
        var body = await response.Content.ReadAsStringAsync();

        return StatusCode((int)response.StatusCode, body);
    }

    /// <summary>
    /// Get transaction detail (proxied to Ingestion Service).
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var client = CreateAuthenticatedClient("ingestion");
        var response = await client.GetAsync($"/api/transactions/{id}");
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
