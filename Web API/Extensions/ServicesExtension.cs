using Econolite_API.Hubs;
using Econolite_API.Modules.Audit.Application;
using Econolite_API.Modules.Audit.Application.Interfaces;
using Econolite_API.Modules.Identity.Application;
using Econolite_API.Modules.Identity.Application.Interfaces;
using Econolite_API.Modules.Identity.Domain.Entities;
using Econolite_API.Modules.Identity.Infrastructure.Jwt;
using Econolite_API.Modules.Intersections.Application;
using Econolite_API.Modules.Intersections.Application.Interfaces;
using Econolite_API.Modules.TrafficEvents.Application;
using Econolite_API.Modules.TrafficEvents.Application.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace Econolite_API.Extensions
{
    public static class ServiceExtension
    {
        public static WebApplicationBuilder AddServices(this WebApplicationBuilder builder)
        {
            builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
            builder.Services.AddScoped<IIntersectionService, IntersectionService>();
            builder.Services.AddScoped<ILoginService, LoginService>();
            builder.Services.AddScoped<ITrafficEventService, TrafficEventService>();
            builder.Services.AddScoped<IAuditService, AuditService>();
            builder.Services.AddSingleton<ITrafficEventPublisher, SignalRTrafficEventPublisher>();
            builder.Services.AddSingleton<IPasswordHasher<ApplicationUser>, PasswordHasher<ApplicationUser>>();

            return builder;
        }
    }
}