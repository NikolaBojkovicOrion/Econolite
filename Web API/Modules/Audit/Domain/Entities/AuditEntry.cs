namespace Econolite_API.Modules.Audit.Domain.Entities;

public sealed class AuditEntry
{
    private AuditEntry()
    {
    }

    public AuditEntry(Guid id, Guid? userId, string action, string entityType, string entityId, DateTimeOffset createdAt)
    {
        Id = id;
        UserId = userId;
        Action = action;
        EntityType = entityType;
        EntityId = entityId;
        CreatedAt = createdAt;
    }

    public Guid Id { get; private set; }
    public Guid? UserId { get; private set; }
    public string Action { get; private set; } = string.Empty;
    public string EntityType { get; private set; } = string.Empty;
    public string EntityId { get; private set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; private set; }
}
