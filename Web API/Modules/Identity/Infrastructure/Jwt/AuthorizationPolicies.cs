namespace Econolite_API.Modules.Identity.Infrastructure.Jwt;

public static class AuthorizationPolicies
{
    public const string CanViewTraffic = "CanViewTraffic";
    public const string CanAcknowledgeTrafficEvent = "CanAcknowledgeTrafficEvent";
    public const string CanResolveCriticalEvent = "CanResolveCriticalEvent";
}
