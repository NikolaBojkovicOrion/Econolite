namespace Econolite_API.Modules.Intersections.Application.Contracts;

public sealed record IntersectionPageQuery(
    int PageNumber,
    int PageSize,
    string? Name,
    string? Status,
    string? Freshness);

public sealed record IntersectionQueryError(string Title, string Detail);

public sealed record IntersectionPageResult(
    IntersectionPageResponse? Page,
    IntersectionQueryError? Error);