namespace Econolite_API.Modules.Intersections.Domain.Entities;

public sealed class Intersection
{
    private Intersection()
    {
    }

    public Intersection(int id, string name, double latitude, double longitude, string status, int? speedMph, DateTimeOffset? lastDetectorUpdate)
    {
        Id = id;
        Name = name;
        Latitude = latitude;
        Longitude = longitude;
        Status = status;
        SpeedMph = speedMph;
        LastDetectorUpdate = lastDetectorUpdate;
    }

    public int Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public double Latitude { get; private set; }
    public double Longitude { get; private set; }
    public string Status { get; private set; } = string.Empty;
    public int? SpeedMph { get; private set; }
    public DateTimeOffset? LastDetectorUpdate { get; private set; }
    public ICollection<Econolite_API.Modules.TrafficEvents.Domain.Entities.TrafficEvent> TrafficEvents { get; private set; } = new List<Econolite_API.Modules.TrafficEvents.Domain.Entities.TrafficEvent>();

    public void RecordDetectorUpdate(DateTimeOffset detectedAt)
    {
        if (LastDetectorUpdate is null || detectedAt > LastDetectorUpdate)
        {
            LastDetectorUpdate = detectedAt;
        }
    }
}
