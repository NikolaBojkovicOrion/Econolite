using Econolite_API.Modules.Identity.Infrastructure.Jwt;
using Econolite_API.Modules.Intersections.Application.Contracts;

namespace Econolite_API.Extensions
{
    public static class AuthorizationExtension
    {
        public static WebApplicationBuilder AddAuthorization(this WebApplicationBuilder builder)
        {
            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy(AuthorizationPolicies.CanViewTraffic, policy =>
                    policy.RequireRole("Operator", "Supervisor", "Admin"));
                options.AddPolicy(AuthorizationPolicies.CanAcknowledgeTrafficEvent, policy =>
                    policy.RequireRole("Operator", "Supervisor", "Admin"));
                options.AddPolicy(AuthorizationPolicies.CanResolveCriticalEvent, policy =>
                    policy.RequireRole("Supervisor", "Admin"));
            });

            return builder;
        }
    }
}