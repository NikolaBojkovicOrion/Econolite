using Econolite_API.Modules.Audit.Application.Contracts;

namespace Econolite_API.Modules.Audit.Application.Interfaces;

public interface IAuditService
{
    Task<IReadOnlyCollection<AuditEntryResponse>> GetRecentAsync(string? entityId, CancellationToken cancellationToken);
    void Record(Guid? userId, string action, string entityType, string entityId);
}