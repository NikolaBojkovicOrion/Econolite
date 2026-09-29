using Econolite_API.Modules.Audit.Domain.Entities;
using Econolite_API.Modules.Identity.Domain.Entities;
using Econolite_API.Modules.Intersections.Domain.Entities;
using Econolite_API.Modules.TrafficEvents.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Econolite_API.Infrastructure.Persistence;

public sealed class EconoliteDbContext(DbContextOptions<EconoliteDbContext> options) : DbContext(options)
{
    public DbSet<Intersection> Intersections => Set<Intersection>();
    public DbSet<TrafficEvent> TrafficEvents => Set<TrafficEvent>();
    public DbSet<ApplicationUser> Users => Set<ApplicationUser>();
    public DbSet<AuditEntry> AuditEntries => Set<AuditEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(EconoliteDbContext).Assembly);
    }
}
