using Econolite_API.Modules.TrafficEvents.Application.Contracts;
using Econolite_API.Modules.TrafficEvents.Application.Interfaces;
using Econolite_API.Modules.Identity.Infrastructure.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.ComponentModel.DataAnnotations;

namespace Econolite_API.Controllers;

[ApiController]
[Authorize(Policy = AuthorizationPolicies.CanViewTraffic)]
[Route("api/events")]
public sealed class TrafficEventsController(
    ITrafficEventService trafficEventService,
    IAuthorizationService authorizationService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<TrafficEventResponse>>> GetActive(
        [FromQuery, Range(1, 100)] int? limit,
        CancellationToken cancellationToken)
    {
        var events = await trafficEventService.GetOpenAsync(limit, cancellationToken);
        return Ok(events);
    }

    [HttpPost("/api/intersections/{intersectionId:int}/events")]
    public async Task<IActionResult> Create(
        int intersectionId,
        CreateTrafficEventRequest request,
        [FromServices] IHostEnvironment environment,
        CancellationToken cancellationToken)
    {
        if (!environment.IsDevelopment())
        {
            return NotFound();
        }

        var result = await trafficEventService.CreateAsync(intersectionId, request, cancellationToken);
        if (result.ErrorStatus is not null)
        {
            return Problem(
                statusCode: result.ErrorStatus,
                title: result.ErrorTitle,
                detail: result.ErrorDetail);
        }

        return StatusCode(StatusCodes.Status201Created, result.Event);
    }

    [HttpPatch("{id:guid}/acknowledge")]
    [Authorize(Policy = AuthorizationPolicies.CanAcknowledgeTrafficEvent)]
    public async Task<IActionResult> Acknowledge(Guid id, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
        {
            return Unauthorized();
        }

        var result = await trafficEventService.AcknowledgeAsync(id, userId, cancellationToken);
        return result.ErrorStatus is null
            ? Ok(result.Event)
            : Problem(statusCode: result.ErrorStatus, title: result.ErrorTitle, detail: result.ErrorDetail);
    }

    [HttpPatch("{id:guid}/resolve")]
    [Authorize(Policy = AuthorizationPolicies.CanResolveCriticalEvent)]
    public async Task<IActionResult> Resolve(Guid id, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
        {
            return Unauthorized();
        }

        var criticalEventPermission = await authorizationService.AuthorizeAsync(
            User,
            AuthorizationPolicies.CanResolveCriticalEvent);
        var result = await trafficEventService.ResolveAsync(
            id,
            userId,
            criticalEventPermission.Succeeded,
            cancellationToken);

        return result.ErrorStatus is null
            ? Ok(result.Event)
            : Problem(statusCode: result.ErrorStatus, title: result.ErrorTitle, detail: result.ErrorDetail);
    }
}
