using Econolite_API.Modules.TrafficEvents.Application.Contracts;
using Econolite_API.Modules.TrafficEvents.Application.Interfaces;
using Econolite_API.Modules.Audit.Application.Contracts;
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
        await SendAsync("TrafficEventCreated", trafficEvent, trafficEvent.Id.ToString(), cancellationToken);
        await SendAsync("IntersectionStatusUpdated", intersectionStatus, trafficEvent.Id.ToString(), cancellationToken);
    }

    public Task PublishUpdatedAsync(TrafficEventResponse trafficEvent, CancellationToken cancellationToken) =>
        SendAsync("TrafficEventUpdated", trafficEvent, trafficEvent.Id.ToString(), cancellationToken);

    public Task PublishAuditEntryCreatedAsync(AuditEntryResponse auditEntry, CancellationToken cancellationToken) =>
        SendAsync("AuditEntryCreated", auditEntry, auditEntry.EntityId, cancellationToken);

    private async Task SendAsync<T>(string messageName, T payload, string entityId, CancellationToken cancellationToken)
    {
        try
        {
            await hubContext.Clients.All.SendAsync(messageName, payload, cancellationToken);
            logger.LogInformation(
                "Published SignalR message {MessageName} for entity {EntityId}.",
                messageName,
                entityId);
        }
        catch (Exception exception)
        {
            logger.LogError(
                exception,
                "Unable to publish SignalR message {MessageName} for entity {EntityId}.",
                messageName,
                entityId);
        }
    }
}