using Econolite_API.Modules.Intersections.Application.Contracts;
using Econolite_API.Modules.Intersections.Application.Interfaces;
using Econolite_API.Modules.Identity.Infrastructure.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Econolite_API.Controllers;

[ApiController]
[Authorize(Policy = AuthorizationPolicies.CanViewTraffic)]
[Route("api/intersections")]
public sealed class IntersectionsController(
    IIntersectionService intersectionService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IntersectionPageResponse>> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? name = null,
        [FromQuery] string? status = null,
        [FromQuery] string? freshness = null,
        CancellationToken cancellationToken = default)
    {
        var result = await intersectionService.GetPageAsync(
            new IntersectionPageQuery(pageNumber, pageSize, name, status, freshness),
            cancellationToken);

        if (result.Error is not null)
        {
            return BadRequest(new ProblemDetails
            {
                Title = result.Error.Title,
                Detail = result.Error.Detail,
                Status = StatusCodes.Status400BadRequest,
            });
        }

        return Ok(result.Page);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<IntersectionDetailResponse>> GetById(int id, CancellationToken cancellationToken)
    {
        var intersection = await intersectionService.GetByIdAsync(id, cancellationToken);
        return intersection is null ? NotFound() : Ok(intersection);
    }
}
