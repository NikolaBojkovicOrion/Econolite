namespace Econolite_API.Modules.Intersections.Application.Contracts;

public sealed class IntersectionMonitoringOptions
{
    public const string SectionName = "IntersectionMonitoring";

    public int FreshThresholdSeconds { get; init; } = 30;
    public int DelayedThresholdSeconds { get; init; } = 120;
}