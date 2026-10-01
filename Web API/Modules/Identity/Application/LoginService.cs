using Econolite_API.Infrastructure.Persistence;
using Econolite_API.Modules.Identity.Application.Contracts;
using Econolite_API.Modules.Identity.Application.Interfaces;
using Econolite_API.Modules.Identity.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Econolite_API.Modules.Identity.Application;

public sealed class LoginService(
    EconoliteDbContext dbContext,
    IPasswordHasher<ApplicationUser> passwordHasher,
    IJwtTokenService tokenService) : ILoginService
{
    public async Task<LoginResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        var user = await dbContext.Users
            .SingleOrDefaultAsync(item => item.Email == request.Email, cancellationToken);

        if (user is null || passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password) == PasswordVerificationResult.Failed)
        {
            return null;
        }

        var token = tokenService.CreateToken(user);
        return new LoginResponse(
            token.Token,
            token.ExpiresAt,
            new CurrentUserResponse(user.Id, user.Email, [user.Role]));
    }
}