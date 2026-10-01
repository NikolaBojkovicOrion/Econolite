using Econolite_API.Modules.TrafficEvents.Application.Contracts;
using Econolite_API.Modules.TrafficEvents.Application.Interfaces;
using Econolite_API.Modules.Identity.Infrastructure.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Econolite_API.Controllers;

[ApiController]
[Authorize(Policy = AuthorizationPolicies.CanViewTraffic)]
[Route("api/events")]
public sealed class TrafficEventsController(ITrafficEventService trafficEventService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<TrafficEventResponse>>> GetActive(CancellationToken cancellationToken)
    {
        var events = await trafficEventService.GetActiveAsync(cancellationToken);
        return Ok(events);
    }
}
