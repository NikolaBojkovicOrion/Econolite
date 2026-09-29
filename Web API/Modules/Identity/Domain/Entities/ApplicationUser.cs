namespace Econolite_API.Modules.Identity.Domain.Entities;

public sealed class ApplicationUser
{
    private ApplicationUser()
    {
    }

    public Guid Id { get; private set; }
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public string Role { get; private set; } = string.Empty;
}
