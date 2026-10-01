namespace Econolite_API.Modules.TrafficEvents.Application.Contracts;

public sealed record IntersectionStatusUpdateResponse(
    int IntersectionId,
    string Status,
    DateTimeOffset? LastDetectorUpdate);