using Econolite_API.Infrastructure.ErrorHandling;

namespace Econolite_API.Extensions
{
    public static class ExceptionHandlingExtension
    {
        public static WebApplicationBuilder AddExceptionHandling(this WebApplicationBuilder builder)
        {
            builder.Services.AddProblemDetails(options =>
            {
                options.CustomizeProblemDetails = context =>
                {
                    context.ProblemDetails.Extensions["traceId"] = context.HttpContext.TraceIdentifier;
                    context.ProblemDetails.Instance = context.HttpContext.Request.Path;
                };
            });
            builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

            return builder;
        }
    }
}