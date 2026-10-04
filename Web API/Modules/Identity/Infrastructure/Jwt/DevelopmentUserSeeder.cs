using Econolite_API.Infrastructure.Persistence;
using Econolite_API.Modules.Identity.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Econolite_API.Modules.Identity.Infrastructure.Jwt;

public static class DevelopmentUserSeeder
{
    public static async Task SeedAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        var dbContext = services.GetRequiredService<EconoliteDbContext>();
        if (await dbContext.Users.AnyAsync(cancellationToken))
        {
            return;
        }

        var user = new ApplicationUser(
            Guid.Parse("5cf6ccf6-71bf-4e74-a1cf-5ec7c2f9e301"),
            "operator@econolite.local",
            string.Empty,
            "Operator");
        var hasher = services.GetRequiredService<IPasswordHasher<ApplicationUser>>();
        user.SetPasswordHash(hasher.HashPassword(user, "Operator123!"));

        var userNikola = new ApplicationUser(
            Guid.Parse("5cf6ccf6-71bf-4e74-a1cf-5ec7c2f9e302"),
            "nikola@econolite.local",
            string.Empty,
            "Admin");
        var hasherNikola = services.GetRequiredService<IPasswordHasher<ApplicationUser>>();
        userNikola.SetPasswordHash(hasherNikola.HashPassword(userNikola, "Operator123!"));

        var userMarko = new ApplicationUser(
            Guid.Parse("5cf6ccf6-71bf-4e74-a1cf-5ec7c2f9e303"),
            "marko@econolite.local",
            string.Empty,
            "Supervisor");
        var hasherMarko = services.GetRequiredService<IPasswordHasher<ApplicationUser>>();
        userMarko.SetPasswordHash(hasherMarko.HashPassword(userMarko, "Operator123!"));

        dbContext.Users.Add(user);
        dbContext.Users.Add(userNikola);
        dbContext.Users.Add(userMarko);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
