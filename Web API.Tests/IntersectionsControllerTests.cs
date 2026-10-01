using Econolite_API.Modules.Intersections.Domain.Entities;

namespace Econolite_API.Tests;

public sealed class IntersectionDomainTests
{
    [Fact]
    public void Intersection_preserves_operational_identity_and_health()
    {
        var intersection = new Intersection(
            101,
            "Harbor Boulevard / Katella Avenue",
            33.8021,
            -117.9143,
            "Healthy",
            null,
            DateTimeOffset.Parse("2026-09-29T09:58:00-07:00"));

        Assert.Equal(101, intersection.Id);
        Assert.Equal("Harbor Boulevard / Katella Avenue", intersection.Name);
        Assert.Equal("Healthy", intersection.Status);
        Assert.Equal(33.8021, intersection.Latitude);
    }

    [Fact]
    public void TrafficEvent_preserves_detector_identity_for_idempotency()
    {
        var trafficEvent = new Econolite_API.Modules.TrafficEvents.Domain.Entities.TrafficEvent(
            Guid.NewGuid(),
            101,
            "Congestion",
            "High",
            "Active",
            DateTimeOffset.UtcNow,
            "DemoDetector",
            "detector-event-101");

        Assert.Equal("DemoDetector", trafficEvent.SourceSystem);
        Assert.Equal("detector-event-101", trafficEvent.ExternalEventId);
    }
}
