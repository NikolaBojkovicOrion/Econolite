using Econolite_API.Infrastructure.Persistence;
using Econolite_API.Modules.Audit.Application.Contracts;
using Econolite_API.Modules.Audit.Application.Interfaces;
using Econolite_API.Modules.Audit.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Econolite_API.Modules.Audit.Application;

public sealed class AuditService(EconoliteDbContext dbContext) : IAuditService
{
    private const int MaximumResults = 100;

    public async Task<IReadOnlyCollection<AuditEntryResponse>> GetRecentAsync(
        string? entityId,
        CancellationToken cancellationToken)
    {
        var entries = dbContext.AuditEntries.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(entityId))
        {
            var selectedEntityId = entityId.Trim();
            entries = entries.Where(entry => entry.EntityId == selectedEntityId);
        }

        return await entries
            .OrderByDescending(entry => entry.CreatedAt)
            .Take(MaximumResults)
            .Select(entry => new AuditEntryResponse(
                entry.Id,
                entry.UserId,
                entry.Action,
                entry.EntityType,
                entry.EntityId,
                entry.CreatedAt))
            .ToListAsync(cancellationToken);
    }

    public void Record(Guid? userId, string action, string entityType, string entityId)
    {
        dbContext.AuditEntries.Add(new AuditEntry(
            Guid.NewGuid(),
            userId,
            action,
            entityType,
            entityId,
            DateTimeOffset.UtcNow));
    }
}