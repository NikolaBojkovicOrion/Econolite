using Econolite_API.Infrastructure.Persistence;
using Econolite_API.Modules.Identity.Domain.Entities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Econolite_API.Tests;

public sealed class ApiFactory : WebApplicationFactory<Program>
{
    private readonly string databaseName = $"EconoliteTrafficTests_{Guid.NewGuid():N}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("Jwt:SigningKey", "testing-only-signing-key-with-enough-length-2026");
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<EconoliteDbContext>>();
            services.RemoveAll<DbContextOptions>();
            services.RemoveAll<IDbContextOptionsConfiguration<EconoliteDbContext>>();

            var connectionString = $"Server=.\\SQLEXPRESS;Database={databaseName};Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=True";
            services.AddDbContext<EconoliteDbContext>(options => options.UseSqlServer(connectionString));

            var serviceProvider = services.BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<EconoliteDbContext>();
            dbContext.Database.EnsureDeleted();
            dbContext.Database.EnsureCreated();

            var user = new ApplicationUser(
                Guid.Parse("5cf6ccf6-71bf-4e74-a1cf-5ec7c2f9e301"),
                "operator@econolite.local",
                string.Empty,
                "Operator");
            user.SetPasswordHash(new PasswordHasher<ApplicationUser>().HashPassword(user, "Operator123!"));
            dbContext.Users.Add(user);
            dbContext.SaveChanges();
        });
    }
}
