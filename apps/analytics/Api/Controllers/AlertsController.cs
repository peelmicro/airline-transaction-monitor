using Analytics.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Analytics.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/alerts")]
public class AlertsController : ControllerBase
{
    private readonly AnalyticsDbContext _context;

    public AlertsController(AnalyticsDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// List alerts with optional filters.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> List(
        [FromQuery] string? airlineCode,
        [FromQuery] string? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken ct = default)
    {
        var query = _context.Alerts.AsQueryable();

        if (!string.IsNullOrEmpty(airlineCode))
            query = query.Where(a => a.AirlineCode == airlineCode);

        if (!string.IsNullOrEmpty(status))
            query = query.Where(a => a.Status == status);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(a => a.FiredAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new
            {
                a.Id,
                a.Code,
                a.AirlineCode,
                RuleName = $"error-rate-{a.WindowMinutes}m",
                a.WindowMinutes,
                a.Threshold,
                a.ActualValue,
                a.Status,
                a.FiredAt,
                a.ResolvedAt
            })
            .ToListAsync(ct);

        return Ok(new { items, totalCount, page, pageSize });
    }
}
