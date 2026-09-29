using Econolite_API.Modules.TrafficEvents.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Econolite_API.Infrastructure.Persistence.Configurations;

public sealed class TrafficEventConfiguration : IEntityTypeConfiguration<TrafficEvent>
{
    public void Configure(EntityTypeBuilder<TrafficEvent> builder)
    {
        builder.ToTable("TrafficEvents");
        builder.HasKey(trafficEvent => trafficEvent.Id);
        builder.Property(trafficEvent => trafficEvent.Type).HasMaxLength(64).IsRequired();
        builder.Property(trafficEvent => trafficEvent.Severity).HasMaxLength(32).IsRequired();
        builder.Property(trafficEvent => trafficEvent.Status).HasMaxLength(32).IsRequired();
        builder.Property(trafficEvent => trafficEvent.SourceSystem).HasMaxLength(64).IsRequired();
        builder.Property(trafficEvent => trafficEvent.ExternalEventId).HasMaxLength(128).IsRequired();
        builder.HasIndex(trafficEvent => new { trafficEvent.SourceSystem, trafficEvent.ExternalEventId }).IsUnique();
        builder.HasIndex(trafficEvent => new { trafficEvent.IntersectionId, trafficEvent.Status });
        builder.HasOne(trafficEvent => trafficEvent.Intersection)
            .WithMany(intersection => intersection.TrafficEvents)
            .HasForeignKey(trafficEvent => trafficEvent.IntersectionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasData(
            new TrafficEvent(Guid.Parse("e5d99f2c-6825-4d4a-8d3f-b37efc89db01"), 102, "Congestion", "High", "Active", DateTimeOffset.Parse("2026-09-29T09:55:00-07:00"), "DemoDetector", "demo-102-001"),
            new TrafficEvent(Guid.Parse("0aef3f85-39c0-4f5c-8ad8-72c7f31f5b02"), 101, "SlowTraffic", "Medium", "Active", DateTimeOffset.Parse("2026-09-29T09:51:00-07:00"), "DemoDetector", "demo-101-001"));
    }
}
