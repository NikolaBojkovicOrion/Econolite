using Econolite_API.Modules.TrafficEvents.Application.Contracts;

namespace Econolite_API.Modules.TrafficEvents.Application.Interfaces;

public interface ITrafficEventService
{
    Task<IReadOnlyCollection<TrafficEventResponse>> GetOpenAsync(CancellationToken cancellationToken);
    Task<TrafficEventCreationResult> CreateAsync(
        int intersectionId,
        CreateTrafficEventRequest request,
        CancellationToken cancellationToken);
    Task<TrafficEventMutationResult> AcknowledgeAsync(
        Guid eventId,
        Guid userId,
        CancellationToken cancellationToken);
    Task<TrafficEventMutationResult> ResolveAsync(
        Guid eventId,
        Guid userId,
        bool canResolveCritical,
        CancellationToken cancellationToken);
}