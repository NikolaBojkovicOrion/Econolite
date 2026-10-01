using Econolite_API.Infrastructure.Persistence;
using Econolite_API.Modules.Intersections.Application.Contracts;
using Econolite_API.Modules.Identity.Infrastructure.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Econolite_API.Controllers;

[ApiController]
[Authorize(Policy = AuthorizationPolicies.CanViewTraffic)]
[Route("api/intersections")]
public sealed class IntersectionsController(
    EconoliteDbContext dbContext,
    IOptions<IntersectionMonitoringOptions> monitoringOptions) : ControllerBase
{
    private const int MaximumPageSize = 100;
    private static readonly string[] HealthStatuses = ["Healthy", "Degraded", "Offline"];

    [HttpGet]
    public async Task<ActionResult<IntersectionPageResponse>> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? name = null,
        [FromQuery] string? status = null,
        [FromQuery] string? freshness = null,
        CancellationToken cancellationToken = default)
    {
        if (pageNumber < 1 || pageSize < 1 || pageSize > MaximumPageSize)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid pagination parameters",
                Detail = $"Page number must be positive and page size must be between 1 and {MaximumPageSize}.",
                Status = StatusCodes.Status400BadRequest,
            });
        }

        var selectedStatus = string.IsNullOrWhiteSpace(status) ? null : status.Trim();
        if (selectedStatus is not null && !HealthStatuses.Contains(selectedStatus, StringComparer.OrdinalIgnoreCase))
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid intersection status",
                Detail = "Status must be Healthy, Degraded, or Offline.",
                Status = StatusCodes.Status400BadRequest,
            });
        }

        var selectedFreshness = string.IsNullOrWhiteSpace(freshness) ? null : freshness.Trim();
        if (selectedFreshness is not null &&
            !new[] { "Fresh", "Delayed", "Stale" }.Contains(selectedFreshness, StringComparer.OrdinalIgnoreCase))
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid detector freshness",
                Detail = "Freshness must be Fresh, Delayed, or Stale.",
                Status = StatusCodes.Status400BadRequest,
            });
        }

        var now = DateTimeOffset.UtcNow;
        var options = monitoringOptions.Value;
        var freshCutoff = now.AddSeconds(-options.FreshThresholdSeconds);
        var delayedCutoff = now.AddSeconds(-options.DelayedThresholdSeconds);
        var allIntersections = dbContext.Intersections.AsNoTracking();

        var summary = new IntersectionSummaryResponse(
            await allIntersections.CountAsync(cancellationToken),
            await allIntersections.CountAsync(intersection => intersection.Status == "Healthy", cancellationToken),
            await allIntersections.CountAsync(
                intersection => intersection.LastDetectorUpdate == null || intersection.LastDetectorUpdate < freshCutoff,
                cancellationToken));

        var query = allIntersections.AsQueryable();
        var selectedName = name?.Trim();
        if (!string.IsNullOrWhiteSpace(selectedName))
        {
            query = query.Where(intersection => intersection.Name.Contains(selectedName));
        }

        if (selectedStatus is not null)
        {
            query = query.Where(intersection => intersection.Status == selectedStatus);
        }

        if (selectedFreshness is not null)
        {
            var normalizedFreshness = selectedFreshness.ToUpperInvariant();
            query = normalizedFreshness switch
            {
                "FRESH" => query.Where(intersection => intersection.LastDetectorUpdate >= freshCutoff),
                "DELAYED" => query.Where(intersection =>
                    intersection.LastDetectorUpdate != null &&
                    intersection.LastDetectorUpdate < freshCutoff &&
                    intersection.LastDetectorUpdate >= delayedCutoff),
                "STALE" => query.Where(intersection =>
                    intersection.LastDetectorUpdate == null || intersection.LastDetectorUpdate < delayedCutoff),
                _ => query,
            };
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var intersections = await query
            .OrderBy(intersection => intersection.Name)
            .ThenBy(intersection => intersection.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(intersection => new
            {
                intersection.Id,
                intersection.Name,
                intersection.Latitude,
                intersection.Longitude,
                intersection.Status,
                intersection.SpeedMph,
                intersection.LastDetectorUpdate,
                ActiveEventCount = intersection.TrafficEvents.Count(trafficEvent => trafficEvent.Status == "Active"),
            })
            .ToListAsync(cancellationToken);

        var items = intersections.Select(intersection => ToResponse(
            intersection.Id,
            intersection.Name,
            intersection.Latitude,
            intersection.Longitude,
            intersection.Status,
            intersection.SpeedMph,
            intersection.LastDetectorUpdate,
            intersection.ActiveEventCount,
            now,
            options)).ToArray();

        return Ok(new IntersectionPageResponse(items, pageNumber, pageSize, totalCount, summary));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<IntersectionDetailResponse>> GetById(int id, CancellationToken cancellationToken)
    {
        var intersection = await dbContext.Intersections
            .AsNoTracking()
            .SingleOrDefaultAsync(item => item.Id == id, cancellationToken);

        if (intersection is null)
        {
            return NotFound();
        }

        var activeEvents = await dbContext.TrafficEvents
            .AsNoTracking()
            .Where(trafficEvent => trafficEvent.IntersectionId == id && trafficEvent.Status == "Active")
            .OrderByDescending(trafficEvent => trafficEvent.DetectedAt)
            .Select(trafficEvent => new IntersectionEventResponse(
                trafficEvent.Id,
                trafficEvent.Type,
                trafficEvent.Severity,
                trafficEvent.Status,
                trafficEvent.DetectedAt))
            .ToListAsync(cancellationToken);

        var now = DateTimeOffset.UtcNow;
        var response = ToResponse(
            intersection.Id,
            intersection.Name,
            intersection.Latitude,
            intersection.Longitude,
            intersection.Status,
            intersection.SpeedMph,
            intersection.LastDetectorUpdate,
            activeEvents.Count,
            now,
            monitoringOptions.Value);

        return Ok(new IntersectionDetailResponse(response, activeEvents));
    }

    private static IntersectionResponse ToResponse(
        int id,
        string name,
        double latitude,
        double longitude,
        string status,
        int? speedMph,
        DateTimeOffset? lastDetectorUpdate,
        int activeEventCount,
        DateTimeOffset now,
        IntersectionMonitoringOptions options) =>
        new(
            id,
            name,
            latitude,
            longitude,
            status,
            speedMph,
            lastDetectorUpdate,
            GetFreshness(lastDetectorUpdate, now, options),
            activeEventCount);

    private static string GetFreshness(
        DateTimeOffset? lastDetectorUpdate,
        DateTimeOffset now,
        IntersectionMonitoringOptions options)
    {
        if (lastDetectorUpdate is null || lastDetectorUpdate < now.AddSeconds(-options.DelayedThresholdSeconds))
        {
            return "Stale";
        }

        return lastDetectorUpdate < now.AddSeconds(-options.FreshThresholdSeconds)
            ? "Delayed"
            : "Fresh";
    }
}
