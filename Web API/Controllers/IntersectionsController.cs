using Econolite_API.Infrastructure.Persistence;
using Econolite_API.Modules.Intersections.Application.Contracts;
using Econolite_API.Modules.Identity.Infrastructure.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Econolite_API.Controllers;

[ApiController]
[Authorize(Policy = AuthorizationPolicies.CanViewTraffic)]
[Route("api/intersections")]
public sealed class IntersectionsController(EconoliteDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<IntersectionResponse>>> GetAll(CancellationToken cancellationToken)
    {
        var intersections = await dbContext.Intersections
            .AsNoTracking()
            .OrderBy(intersection => intersection.Name)
            .Select(intersection => new IntersectionResponse(
                intersection.Id,
                intersection.Name,
                intersection.Latitude,
                intersection.Longitude,
                intersection.Status,
                intersection.LastDetectorUpdate))
            .ToListAsync(cancellationToken);

        return Ok(intersections);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<IntersectionResponse>> GetById(int id, CancellationToken cancellationToken)
    {
        var intersection = await dbContext.Intersections
            .AsNoTracking()
            .Where(item => item.Id == id)
            .Select(item => new IntersectionResponse(
                item.Id,
                item.Name,
                item.Latitude,
                item.Longitude,
                item.Status,
                item.LastDetectorUpdate))
            .SingleOrDefaultAsync(cancellationToken);

        return intersection is null ? NotFound() : Ok(intersection);
    }
}
