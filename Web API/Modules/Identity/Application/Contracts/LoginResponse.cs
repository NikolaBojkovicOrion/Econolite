namespace Econolite_API.Modules.Identity.Application.Contracts;

public sealed record LoginResponse(
    string AccessToken,
    DateTimeOffset ExpiresAt,
    CurrentUserResponse User);
