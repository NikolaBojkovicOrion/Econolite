using Econolite_API.Modules.Identity.Infrastructure.Jwt;
using Econolite_API.Modules.Intersections.Application.Contracts;

namespace Econolite_API.Extensions
{
    public static class OptionsExtension
    {
        public static WebApplicationBuilder AddOptions(this WebApplicationBuilder builder)
        {
            builder.Services.AddOptions<IntersectionMonitoringOptions>()
                .Bind(builder.Configuration.GetSection(IntersectionMonitoringOptions.SectionName))
                .Validate(options => options.FreshThresholdSeconds > 0 &&
                                    options.DelayedThresholdSeconds > options.FreshThresholdSeconds,
                    "Detector freshness thresholds must be positive and delayed must exceed fresh.")
                .ValidateOnStart();
            builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));

            return builder;
        }
    }
}