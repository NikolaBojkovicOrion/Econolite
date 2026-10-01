using Econolite_API.Modules.TrafficEvents.Application.Contracts;
using Econolite_API.Modules.TrafficEvents.Application.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace Econolite_API.Hubs;

public sealed class SignalRTrafficEventPublisher(
    IHubContext<TrafficHub> hubContext,
    ILogger<SignalRTrafficEventPublisher> logger) : ITrafficEventPublisher
{
    public async Task PublishCreatedAsync(
        TrafficEventResponse trafficEvent,
        IntersectionStatusUpdateResponse intersectionStatus,
        CancellationToken cancellationToken)
    {
        await SendAsync("TrafficEventCreated", trafficEvent, trafficEvent.Id, cancellationToken);
        await SendAsync("IntersectionStatusUpdated", intersectionStatus, trafficEvent.Id, cancellationToken);
    }

    public Task PublishUpdatedAsync(TrafficEventResponse trafficEvent, CancellationToken cancellationToken) =>
        SendAsync("TrafficEventUpdated", trafficEvent, trafficEvent.Id, cancellationToken);

    private async Task SendAsync<T>(string messageName, T payload, Guid trafficEventId, CancellationToken cancellationToken)
    {
        try
        {
            await hubContext.Clients.All.SendAsync(messageName, payload, cancellationToken);
            logger.LogInformation(
                "Published SignalR message {MessageName} for traffic event {TrafficEventId}.",
                messageName,
                trafficEventId);
        }
        catch (Exception exception)
        {
            logger.LogError(
                exception,
                "Unable to publish SignalR message {MessageName} for traffic event {TrafficEventId}.",
                messageName,
                trafficEventId);
        }
    }
}