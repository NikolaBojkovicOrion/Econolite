using Econolite_API.Infrastructure.Persistence;
using Econolite_API.Modules.TrafficEvents.Application.Contracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Econolite_API.Controllers;

[ApiController]
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
