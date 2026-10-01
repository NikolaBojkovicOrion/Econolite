using Econolite_API.Infrastructure.Persistence;
using Econolite_API.Modules.Intersections.Application.Contracts;
using Econolite_API.Modules.Intersections.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Econolite_API.Modules.Intersections.Application;

public sealed class IntersectionService(
    EconoliteDbContext dbContext,
    IOptions<IntersectionMonitoringOptions> monitoringOptions) : IIntersectionService
{
    private const int MaximumPageSize = 100;
    private static readonly string[] HealthStatuses = ["Healthy", "Degraded", "Offline"];
    private static readonly string[] FreshnessStates = ["Fresh", "Delayed", "Stale"];

    public async Task<IntersectionPageResult> GetPageAsync(
        IntersectionPageQuery request,
        CancellationToken cancellationToken)
    {
        var validationError = Validate(request);
        if (validationError is not null)
        {
            return new IntersectionPageResult(null, validationError);
        }

        var selectedStatus = Normalize(request.Status);
        var selectedFreshness = Normalize(request.Freshness);
        var selectedName = request.Name?.Trim();
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

        var intersectionsQuery = allIntersections.AsQueryable();
        if (!string.IsNullOrWhiteSpace(selectedName))
        {
            intersectionsQuery = intersectionsQuery.Where(intersection => intersection.Name.Contains(selectedName));
        }

        if (selectedStatus is not null)
        {
            intersectionsQuery = intersectionsQuery.Where(intersection => intersection.Status == selectedStatus);
        }

        intersectionsQuery = ApplyFreshnessFilter(
            intersectionsQuery,
            selectedFreshness,
            freshCutoff,
            delayedCutoff);

        var totalCount = await intersectionsQuery.CountAsync(cancellationToken);
        var intersections = await intersectionsQuery
            .OrderBy(intersection => intersection.Name)
            .ThenBy(intersection => intersection.Id)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
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

        return new IntersectionPageResult(
            new IntersectionPageResponse(items, request.PageNumber, request.PageSize, totalCount, summary),
            null);
    }

    public async Task<IntersectionDetailResponse?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var intersection = await dbContext.Intersections
            .AsNoTracking()
            .SingleOrDefaultAsync(item => item.Id == id, cancellationToken);

        if (intersection is null)
        {
            return null;
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

        var options = monitoringOptions.Value;
        var response = ToResponse(
            intersection.Id,
            intersection.Name,
            intersection.Latitude,
            intersection.Longitude,
            intersection.Status,
            intersection.SpeedMph,
            intersection.LastDetectorUpdate,
            activeEvents.Count,
            DateTimeOffset.UtcNow,
            options);

        return new IntersectionDetailResponse(response, activeEvents);
    }

    private static IntersectionQueryError? Validate(IntersectionPageQuery query)
    {
        if (query.PageNumber < 1 || query.PageSize < 1 || query.PageSize > MaximumPageSize)
        {
            return new IntersectionQueryError(
                "Invalid pagination parameters",
                $"Page number must be positive and page size must be between 1 and {MaximumPageSize}.");
        }

        var selectedStatus = Normalize(query.Status);
        if (selectedStatus is not null && !HealthStatuses.Contains(selectedStatus, StringComparer.OrdinalIgnoreCase))
        {
            return new IntersectionQueryError(
                "Invalid intersection status",
                "Status must be Healthy, Degraded, or Offline.");
        }

        var selectedFreshness = Normalize(query.Freshness);
        if (selectedFreshness is not null && !FreshnessStates.Contains(selectedFreshness, StringComparer.OrdinalIgnoreCase))
        {
            return new IntersectionQueryError(
                "Invalid detector freshness",
                "Freshness must be Fresh, Delayed, or Stale.");
        }

        return null;
    }

    private static IQueryable<Modules.Intersections.Domain.Entities.Intersection> ApplyFreshnessFilter(
        IQueryable<Modules.Intersections.Domain.Entities.Intersection> query,
        string? freshness,
        DateTimeOffset freshCutoff,
        DateTimeOffset delayedCutoff) => freshness?.ToUpperInvariant() switch
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

    private static string? Normalize(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

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