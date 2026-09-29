namespace Econolite_API.Modules.TrafficEvents.Application.Contracts;

public sealed record TrafficEventResponse(
    Guid Id,
    int IntersectionId,
    string Type,
    string Severity,
    string Status,
    DateTimeOffset DetectedAt);