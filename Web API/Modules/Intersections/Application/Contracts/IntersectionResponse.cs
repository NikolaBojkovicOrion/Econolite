namespace Econolite_API.Modules.Intersections.Application.Contracts;

public sealed record IntersectionResponse(
    int Id,
    string Name,
    double Latitude,
    double Longitude,
    string Status,
    int? SpeedMph,
    DateTimeOffset? LastDetectorUpdate,
    string Freshness,
    int ActiveEventCount);

public sealed record IntersectionPageResponse(
    IReadOnlyCollection<IntersectionResponse> Items,
    int PageNumber,
    int PageSize,
    int TotalCount,
    IntersectionSummaryResponse Summary);

public sealed record IntersectionSummaryResponse(
    int TotalCount,
    int HealthyCount,
    int DelayedOrStaleCount);

public sealed record IntersectionDetailResponse(
    IntersectionResponse Intersection,
    IReadOnlyCollection<IntersectionEventResponse> ActiveEvents);

public sealed record IntersectionEventResponse(
    Guid Id,
    string Type,
    string Severity,
    string Status,
    DateTimeOffset DetectedAt);