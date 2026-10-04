namespace Econolite_API.Extensions
{
    public static class CorsExtension
    {
        public static WebApplicationBuilder AddCors(this WebApplicationBuilder builder)
        {
            var allowedOrigins = builder.Configuration
                .GetSection("Cors:AllowedOrigins")
                .GetChildren()
                .Select(section => section.Value!)
                .Where(origin => !string.IsNullOrWhiteSpace(origin))
                .ToArray();

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("ClientApp", policy =>
                {
                    policy.WithOrigins(allowedOrigins)
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                });
            });

            return builder;
        }
    }
}