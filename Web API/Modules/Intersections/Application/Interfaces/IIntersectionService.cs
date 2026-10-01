using Econolite_API.Modules.Intersections.Application.Contracts;

namespace Econolite_API.Modules.Intersections.Application.Interfaces;

public interface IIntersectionService
{
    Task<IntersectionPageResult> GetPageAsync(
        IntersectionPageQuery query,
        CancellationToken cancellationToken);

    Task<IntersectionDetailResponse?> GetByIdAsync(int id, CancellationToken cancellationToken);
}