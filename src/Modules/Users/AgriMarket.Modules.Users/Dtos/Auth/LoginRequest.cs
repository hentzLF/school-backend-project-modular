using System.ComponentModel.DataAnnotations;

namespace AgriMarket.Modules.Users.Dtos.Auth;

internal sealed class LoginRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; init; } = default!;

    [Required]
    public string Password { get; init; } = default!;
}
