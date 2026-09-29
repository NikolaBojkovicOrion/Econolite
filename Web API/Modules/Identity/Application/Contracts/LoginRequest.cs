using System.ComponentModel.DataAnnotations;

namespace Econolite_API.Modules.Identity.Application.Contracts;

public sealed record LoginRequest(
    [param: Required, EmailAddress] string Email,
    [param: Required, MinLength(8)] string Password);
