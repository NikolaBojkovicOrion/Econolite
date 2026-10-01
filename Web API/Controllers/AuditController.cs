using Econolite_API.Modules.Audit.Application.Contracts;
using Econolite_API.Modules.Audit.Application.Interfaces;
using Econolite_API.Modules.Identity.Infrastructure.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Econolite_API.Controllers;

[ApiController]
[Authorize(Policy = AuthorizationPolicies.CanViewTraffic)]
[Route("api/audit")]
public sealed class AuditController(IAuditService auditService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<AuditEntryResponse>>> GetRecent(
        [FromQuery] string? entityId,
        CancellationToken cancellationToken)
    {
        var entries = await auditService.GetRecentAsync(entityId, cancellationToken);
        return Ok(entries);
    }
}