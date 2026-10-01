namespace Econolite_API.Modules.TrafficEvents.Application.Contracts;

public sealed record CreateTrafficEventRequest(
    string Type,
    string Severity,
    DateTimeOffset DetectedAt,
    string SourceSystem,
    string ExternalEventId);

public sealed record TrafficEventResponse(
    Guid Id,
    int IntersectionId,
    string Type,
    string Severity,
    string Status,
    DateTimeOffset DetectedAt);

public sealed record TrafficEventCreationResult(
    TrafficEventResponse? Event,
    int? ErrorStatus,
    string? ErrorTitle,
    string? ErrorDetail);

public sealed record TrafficEventMutationResult(
    TrafficEventResponse? Event,
    int? ErrorStatus,
    string? ErrorTitle,
    string? ErrorDetail);