using Econolite_API.Infrastructure.Persistence;
using Econolite_API.Modules.Audit.Application.Interfaces;
using Econolite_API.Modules.TrafficEvents.Application.Contracts;
using Econolite_API.Modules.TrafficEvents.Application.Interfaces;
using Econolite_API.Modules.TrafficEvents.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace Econolite_API.Modules.TrafficEvents.Application;

public sealed class TrafficEventService(EconoliteDbContext dbContext, IAuditService auditService) : ITrafficEventService
{
    private static readonly string[] SupportedTypes = ["Congestion", "SlowTraffic", "Incident"];
    private static readonly string[] SupportedSeverities = ["Low", "Medium", "High", "Critical"];

    public async Task<IReadOnlyCollection<TrafficEventResponse>> GetOpenAsync(CancellationToken cancellationToken)
    {
        return await dbContext.TrafficEvents
            .AsNoTracking()
            .Where(trafficEvent => trafficEvent.Status != "Resolved")
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

    public async Task<TrafficEventCreationResult> CreateAsync(
        int intersectionId,
        CreateTrafficEventRequest request,
        CancellationToken cancellationToken)
    {
        var sourceSystem = request.SourceSystem?.Trim();
        var externalEventId = request.ExternalEventId?.Trim();
        var eventType = SupportedTypes.FirstOrDefault(
            supportedType => string.Equals(supportedType, request.Type?.Trim(), StringComparison.OrdinalIgnoreCase));
        if (eventType is null)
        {
            return Failure("Invalid traffic event type", "Type must be Congestion, SlowTraffic, or Incident.", StatusCodes.Status400BadRequest);
        }

        var severity = SupportedSeverities.FirstOrDefault(
            supportedSeverity => string.Equals(supportedSeverity, request.Severity?.Trim(), StringComparison.OrdinalIgnoreCase));
        if (severity is null)
        {
            return Failure("Invalid traffic event severity", "Severity must be Low, Medium, High, or Critical.", StatusCodes.Status400BadRequest);
        }

        if (request.DetectedAt > DateTimeOffset.UtcNow.AddMinutes(5))
        {
            return Failure("Invalid detection time", "DetectedAt cannot be more than five minutes in the future.", StatusCodes.Status400BadRequest);
        }

        if (string.IsNullOrWhiteSpace(sourceSystem) || sourceSystem.Length > 64 ||
            string.IsNullOrWhiteSpace(externalEventId) || externalEventId.Length > 128)
        {
            return Failure("Invalid detector event identity", "SourceSystem and ExternalEventId are required and must fit their supported lengths.", StatusCodes.Status400BadRequest);
        }

        var intersection = await dbContext.Intersections
            .SingleOrDefaultAsync(item => item.Id == intersectionId, cancellationToken);
        if (intersection is null)
        {
            return Failure("Intersection not found", $"Intersection {intersectionId} does not exist.", StatusCodes.Status404NotFound);
        }

        var duplicateExists = await dbContext.TrafficEvents.AnyAsync(
            trafficEvent => trafficEvent.SourceSystem == sourceSystem &&
                            trafficEvent.ExternalEventId == externalEventId,
            cancellationToken);
        if (duplicateExists)
        {
            return Failure("Duplicate detector event", "An event with this source system and external event ID has already been processed.", StatusCodes.Status409Conflict);
        }

        var trafficEvent = new TrafficEvent(
            Guid.NewGuid(),
            intersectionId,
            eventType,
            severity,
            "Active",
            request.DetectedAt,
            sourceSystem,
            externalEventId);

        intersection.RecordDetectorUpdate(request.DetectedAt);
        dbContext.TrafficEvents.Add(trafficEvent);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            dbContext.Entry(trafficEvent).State = EntityState.Detached;
            var duplicateAfterSave = await dbContext.TrafficEvents.AnyAsync(
                existingEvent => existingEvent.SourceSystem == sourceSystem &&
                                 existingEvent.ExternalEventId == externalEventId,
                cancellationToken);
            if (!duplicateAfterSave)
            {
                throw;
            }

            return Failure("Duplicate detector event", "An event with this source system and external event ID has already been processed.", StatusCodes.Status409Conflict);
        }

        return new TrafficEventCreationResult(ToResponse(trafficEvent), null, null, null);
    }

    public Task<TrafficEventMutationResult> AcknowledgeAsync(
        Guid eventId,
        Guid userId,
        CancellationToken cancellationToken) =>
        ChangeStatusAsync(eventId, userId, resolve: false, canResolveCritical: false, cancellationToken);

    public Task<TrafficEventMutationResult> ResolveAsync(
        Guid eventId,
        Guid userId,
        bool canResolveCritical,
        CancellationToken cancellationToken) =>
        ChangeStatusAsync(eventId, userId, resolve: true, canResolveCritical, cancellationToken);

    private async Task<TrafficEventMutationResult> ChangeStatusAsync(
        Guid eventId,
        Guid userId,
        bool resolve,
        bool canResolveCritical,
        CancellationToken cancellationToken)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(
            IsolationLevel.Serializable,
            cancellationToken);

        var trafficEvent = await dbContext.TrafficEvents
            .SingleOrDefaultAsync(item => item.Id == eventId, cancellationToken);
        if (trafficEvent is null)
        {
            return MutationFailure("Traffic event not found", $"Traffic event {eventId} does not exist.", StatusCodes.Status404NotFound);
        }

        if (!resolve && trafficEvent.Status == "Acknowledged")
        {
            return new TrafficEventMutationResult(ToResponse(trafficEvent), null, null, null);
        }

        if (resolve && string.Equals(trafficEvent.Severity, "Critical", StringComparison.OrdinalIgnoreCase) && !canResolveCritical)
        {
            return MutationFailure("Forbidden", "Only supervisors and administrators can resolve critical events.", StatusCodes.Status403Forbidden);
        }

        var changed = resolve ? trafficEvent.Resolve() : trafficEvent.Acknowledge();
        if (!changed)
        {
            var requestedAction = resolve ? "resolved" : "acknowledged";
            return MutationFailure(
                "Invalid traffic event state",
                $"A traffic event in state {trafficEvent.Status} cannot be {requestedAction}.",
                StatusCodes.Status409Conflict);
        }

        auditService.Record(
            userId,
            resolve ? "TrafficEvent.Resolved" : "TrafficEvent.Acknowledged",
            "TrafficEvent",
            trafficEvent.Id.ToString());

        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return new TrafficEventMutationResult(ToResponse(trafficEvent), null, null, null);
    }

    private static TrafficEventResponse ToResponse(TrafficEvent trafficEvent) => new(
        trafficEvent.Id,
        trafficEvent.IntersectionId,
        trafficEvent.Type,
        trafficEvent.Severity,
        trafficEvent.Status,
        trafficEvent.DetectedAt);

    private static TrafficEventCreationResult Failure(string title, string detail, int status) =>
        new(null, status, title, detail);

    private static TrafficEventMutationResult MutationFailure(string title, string detail, int status) =>
        new(null, status, title, detail);
}