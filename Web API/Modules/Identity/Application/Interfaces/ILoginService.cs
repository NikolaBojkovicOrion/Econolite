using Econolite_API.Modules.Identity.Application.Contracts;

namespace Econolite_API.Modules.Identity.Application.Interfaces;

public interface ILoginService
{
    Task<LoginResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken);
}