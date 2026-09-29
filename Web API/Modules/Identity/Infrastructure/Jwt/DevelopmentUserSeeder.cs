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

        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
