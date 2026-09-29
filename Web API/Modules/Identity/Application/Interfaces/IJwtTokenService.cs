using Econolite_API.Modules.Identity.Domain.Entities;

namespace Econolite_API.Modules.Identity.Application.Interfaces;

public interface IJwtTokenService
{
    (string Token, DateTimeOffset ExpiresAt) CreateToken(ApplicationUser user);
}
