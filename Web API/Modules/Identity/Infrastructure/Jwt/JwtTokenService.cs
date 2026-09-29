using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Econolite_API.Modules.Identity.Application.Interfaces;
using Econolite_API.Modules.Identity.Domain.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Econolite_API.Modules.Identity.Infrastructure.Jwt;

public sealed class JwtTokenService(IOptions<JwtOptions> options) : IJwtTokenService
{
    public (string Token, DateTimeOffset ExpiresAt) CreateToken(ApplicationUser user)
    {
        var jwtOptions = options.Value;
        if (string.IsNullOrWhiteSpace(jwtOptions.SigningKey))
        {
            throw new InvalidOperationException("Jwt:SigningKey must be configured.");
        }

        var expiresAt = DateTimeOffset.UtcNow.AddMinutes(jwtOptions.ExpirationMinutes);
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Email),
            new Claim(ClaimTypes.Role, user.Role),
        };

        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SigningKey)),
            SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: jwtOptions.Issuer,
            audience: jwtOptions.Audience,
            claims: claims,
            expires: expiresAt.UtcDateTime,
            signingCredentials: credentials);

        return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }
}
