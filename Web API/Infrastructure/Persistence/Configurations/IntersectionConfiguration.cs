using Econolite_API.Modules.Intersections.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Econolite_API.Infrastructure.Persistence.Configurations;

public sealed class IntersectionConfiguration : IEntityTypeConfiguration<Intersection>
{
    public void Configure(EntityTypeBuilder<Intersection> builder)
    {
        builder.ToTable("Intersections");
        builder.HasKey(intersection => intersection.Id);
        builder.Property(intersection => intersection.Name).HasMaxLength(160).IsRequired();
        builder.Property(intersection => intersection.Status).HasMaxLength(32).IsRequired();
        builder.Property(intersection => intersection.Latitude).HasPrecision(9, 6);
        builder.Property(intersection => intersection.Longitude).HasPrecision(9, 6);
        builder.HasIndex(intersection => intersection.Status);

        builder.HasData(
            new Intersection(101, "Harbor Boulevard / Katella Avenue", 33.8021, -117.9143, "Healthy", DateTimeOffset.Parse("2026-09-29T09:58:00-07:00")),
            new Intersection(102, "Main Street / Broadway", 33.8353, -117.9145, "Degraded", DateTimeOffset.Parse("2026-09-29T09:55:00-07:00")),
            new Intersection(103, "Lincoln Avenue / Euclid Street", 33.8366, -117.9417, "Healthy", DateTimeOffset.Parse("2026-09-29T09:59:00-07:00")));
    }
}
