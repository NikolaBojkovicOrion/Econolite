namespace Econolite_API.Extensions
{
    public static class SwaggerExtension
    {
        public static WebApplicationBuilder AddSwaggerGen(this WebApplicationBuilder builder)
        {
            builder.Services.AddSwaggerGen();
            builder.Services.AddOpenApi();
            builder.Services.AddHealthChecks();

            return builder;
        }
    }
}