using Econolite_API.Modules.Identity.Infrastructure.Jwt;
using Econolite_API.Extensions;
using Econolite_API.Infrastructure.Logging;

using Econolite_API.Hubs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSignalR();
builder.AddExceptionHandling();
builder.AddSwaggerGen();
builder.AddSqlDatabase();
builder.AddOptions();
builder.AddServices();
builder.AddAuthentication();
builder.AddAuthorization();
builder.AddCors();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    await DevelopmentUserSeeder.SeedAsync(scope.ServiceProvider);
}

app.UseExceptionHandler();
app.UseMiddleware<RequestLoggingMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseCors("ClientApp");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<TrafficHub>("/hubs/traffic");
app.MapHealthChecks("/health");
app.MapGet("/version", (IHostEnvironment environment) => Results.Ok(new
{
    service = "Econolite API",
    version = "1.0.0",
    environment = environment.EnvironmentName,
}));

app.Run();


public partial class Program
{
}
