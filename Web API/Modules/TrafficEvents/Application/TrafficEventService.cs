using Econolite_API.Infrastructure.Persistence;
using Econolite_API.Modules.TrafficEvents.Application.Contracts;
using Econolite_API.Modules.TrafficEvents.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Econolite_API.Modules.TrafficEvents.Application;

public sealed class TrafficEventService(EconoliteDbContext dbContext) : ITrafficEventService
{
    public async Task<IReadOnlyCollection<TrafficEventResponse>> GetActiveAsync(CancellationToken cancellationToken)
    {
        return await dbContext.TrafficEvents
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
    }
}