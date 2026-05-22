namespace AgriMarket.Modules.Users.Entities;

internal class AppUser
{
    public Guid Id { get; set; }

    public string Email { get; set; } = default!;

    public string PasswordHash { get; set; } = default!;

    public DateTime? LockoutEnd { get; set; }

    public DateTime CreatedAt { get; set; }

    // Navigation
    public UserProfile? Profile { get; set; }
    public ICollection<UserRole>? Roles { get; set; }
    public ICollection<RefreshToken>? RefreshTokens { get; set; }
}
