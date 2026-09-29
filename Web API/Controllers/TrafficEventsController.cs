using Econolite_API.Infrastructure.Persistence;
using Econolite_API.Modules.TrafficEvents.Application.Contracts;
using Econolite_API.Modules.Identity.Infrastructure.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Econolite_API.Controllers;

[ApiController]
[Authorize(Policy = AuthorizationPolicies.CanViewTraffic)]
[Route("api/events")]
public sealed class TrafficEventsController(EconoliteDbContext dbContext) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<TrafficEventResponse>>> GetActive(CancellationToken cancellationToken)
    {
        var events = await dbContext.TrafficEvents
            .AsNoTracking()
            .Where(trafficEvent => trafficEvent.Status == "Active")
            .OrderByDescending(trafficEvent => trafficEvent.DetectedAt)
            .Select(trafficEvent => new TrafficEventResponse(
                trafficEvent.Id,
                trafficEvent.IntersectionId,
                trafficEvent.Type,
                trafficEvent.Severity,
                trafficEvent.Status,
                trafficEvent.DetectedAt))
            .ToListAsync(cancellationToken);

        return Ok(events);
    }
}
