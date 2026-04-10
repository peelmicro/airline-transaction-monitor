using Analytics.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Analytics.Api.Controllers;

[ApiController]
public class MetricsController : ControllerBase
{
    private readonly AnalyticsDbContext _context;

    public MetricsController(AnalyticsDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Get rolling metrics for a specific airline across all time windows.
    /// </summary>
    [HttpGet("api/airlines/{code}/metrics")]
    public async Task<IActionResult> GetAirlineMetrics(string code, CancellationToken ct)
    {
        var metrics = await _context.MetricSnapshots
            .Where(m => m.AirlineCode == code)
            .OrderBy(m => m.WindowMinutes)
            .Select(m => new
            {
                m.AirlineCode,
                m.WindowMinutes,
                m.TransactionCount,
                m.TotalVolume,
                m.CurrencyCode,
                m.ErrorCount,
                m.ErrorRate,
                m.LatencyP95Ms,
                m.LatencyP99Ms,
                m.UpdatedAt
            })
            .ToListAsync(ct);

        return Ok(metrics);
    }
}
