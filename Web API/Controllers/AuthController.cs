using Econolite_API.Modules.Identity.Application.Contracts;
using Econolite_API.Modules.Identity.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Econolite_API.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(ILoginService loginService) : ControllerBase
{
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        var response = await loginService.LoginAsync(request, cancellationToken);
        return response is null
            ? Unauthorized(new { message = "Invalid email or password." })
            : Ok(response);
    }
}
