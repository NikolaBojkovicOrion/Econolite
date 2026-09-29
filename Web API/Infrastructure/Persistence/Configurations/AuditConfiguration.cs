using Econolite_API.Modules.Audit.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Econolite_API.Infrastructure.Persistence.Configurations;

public sealed class AuditConfiguration : IEntityTypeConfiguration<AuditEntry>
{
    public void Configure(EntityTypeBuilder<AuditEntry> builder)
    {
        builder.ToTable("AuditEntries");
        builder.HasKey(entry => entry.Id);
        builder.Property(entry => entry.Action).HasMaxLength(128).IsRequired();
        builder.Property(entry => entry.EntityType).HasMaxLength(128).IsRequired();
        builder.Property(entry => entry.EntityId).HasMaxLength(128).IsRequired();
        builder.HasIndex(entry => entry.CreatedAt);
    }
}
