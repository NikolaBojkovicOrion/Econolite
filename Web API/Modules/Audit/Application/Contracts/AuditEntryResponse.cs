namespace Econolite_API.Modules.Audit.Application.Contracts;

public sealed record AuditEntryResponse(
    Guid Id,
    Guid? UserId,
    string Action,
    string EntityType,
    string EntityId,
    DateTimeOffset CreatedAt);