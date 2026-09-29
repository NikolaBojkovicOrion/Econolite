namespace Econolite_API.Modules.Identity.Domain.Entities;

public sealed class ApplicationUser
{
    private ApplicationUser()
    {
    }

    public ApplicationUser(Guid id, string email, string passwordHash, string role)
    {
        Id = id;
        Email = email;
        PasswordHash = passwordHash;
        Role = role;
    }

    public Guid Id { get; private set; }
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public string Role { get; private set; } = string.Empty;

    public void SetPasswordHash(string passwordHash)
    {
        PasswordHash = passwordHash;
    }
}
