using Econolite_API.Modules.TrafficEvents.Application.Contracts;
using Econolite_API.Modules.Audit.Application.Contracts;

namespace Econolite_API.Modules.TrafficEvents.Application.Interfaces;

public interface ITrafficEventPublisher
{
    Task PublishCreatedAsync(
        TrafficEventResponse trafficEvent,
        IntersectionStatusUpdateResponse intersectionStatus,
        CancellationToken cancellationToken);

    Task PublishUpdatedAsync(TrafficEventResponse trafficEvent, CancellationToken cancellationToken);

    Task PublishAuditEntryCreatedAsync(AuditEntryResponse auditEntry, CancellationToken cancellationToken);
}