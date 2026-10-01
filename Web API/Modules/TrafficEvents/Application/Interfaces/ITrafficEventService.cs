using Econolite_API.Modules.TrafficEvents.Application.Contracts;

namespace Econolite_API.Modules.TrafficEvents.Application.Interfaces;

public interface ITrafficEventService
{
    Task<IReadOnlyCollection<TrafficEventResponse>> GetActiveAsync(CancellationToken cancellationToken);
}