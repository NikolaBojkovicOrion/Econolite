namespace Econolite_API.Modules.Identity.Application.Contracts;

public sealed record CurrentUserResponse(
    Guid Id,
    string Email,
    IReadOnlyCollection<string> Roles);