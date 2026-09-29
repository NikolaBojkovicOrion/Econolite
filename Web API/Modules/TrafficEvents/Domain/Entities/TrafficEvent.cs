namespace Econolite_API.Modules.TrafficEvents.Domain.Entities;

public sealed class TrafficEvent
{
    private TrafficEvent()
    {
    }

    public TrafficEvent(Guid id, int intersectionId, string type, string severity, string status, DateTimeOffset detectedAt, string sourceSystem, string externalEventId)
    {
        Id = id;
        IntersectionId = intersectionId;
        Type = type;
        Severity = severity;
        Status = status;
        DetectedAt = detectedAt;
        SourceSystem = sourceSystem;
        ExternalEventId = externalEventId;
    }

    public Guid Id { get; private set; }
    public int IntersectionId { get; private set; }
    public string Type { get; private set; } = string.Empty;
    public string Severity { get; private set; } = string.Empty;
    public string Status { get; private set; } = string.Empty;
    public DateTimeOffset DetectedAt { get; private set; }
    public string SourceSystem { get; private set; } = string.Empty;
    public string ExternalEventId { get; private set; } = string.Empty;
    public Econolite_API.Modules.Intersections.Domain.Entities.Intersection Intersection { get; private set; } = null!;
}
