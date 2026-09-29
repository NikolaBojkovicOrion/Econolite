using Econolite_API.Infrastructure.Persistence;
using Econolite_API.Modules.Identity.Application.Contracts;
using Econolite_API.Modules.Identity.Application.Interfaces;
using Econolite_API.Modules.Identity.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Econolite_API.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(
    EconoliteDbContext dbContext,
    IPasswordHasher<ApplicationUser> passwordHasher,
    IJwtTokenService tokenService) : ControllerBase
{
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        var user = await dbContext.Users
            .SingleOrDefaultAsync(item => item.Email == request.Email, cancellationToken);

        if (user is null || passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password) == PasswordVerificationResult.Failed)
        {
            return Unauthorized(new { message = "Invalid email or password." });
        }

        var token = tokenService.CreateToken(user);
        return Ok(new LoginResponse(
            token.Token,
            token.ExpiresAt,
            new CurrentUserResponse(user.Id, user.Email, [user.Role])));
    }
}
