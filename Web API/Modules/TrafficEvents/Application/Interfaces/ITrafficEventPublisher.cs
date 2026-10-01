using Econolite_API.Modules.TrafficEvents.Application.Contracts;

namespace Econolite_API.Modules.TrafficEvents.Application.Interfaces;

public interface ITrafficEventPublisher
{
    Task PublishCreatedAsync(
        TrafficEventResponse trafficEvent,
        IntersectionStatusUpdateResponse intersectionStatus,
        CancellationToken cancellationToken);

    Task PublishUpdatedAsync(TrafficEventResponse trafficEvent, CancellationToken cancellationToken);
}