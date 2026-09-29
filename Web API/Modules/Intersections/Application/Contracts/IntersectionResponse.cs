namespace Econolite_API.Modules.Intersections.Application.Contracts;

public sealed record IntersectionResponse(
    int Id,
    string Name,
    double Latitude,
    double Longitude,
    string Status,
    DateTimeOffset? LastDetectorUpdate);