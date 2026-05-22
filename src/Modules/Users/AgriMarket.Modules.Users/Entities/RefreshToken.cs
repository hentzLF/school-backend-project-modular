namespace AgriMarket.Modules.Users.Entities;

internal sealed class RefreshToken
{
    public Guid Id { get; set; }

    public string Token { get; set; } = default!;

    public Guid AppUserId { get; set; }

    public DateTime ExpiresAt { get; set; }

    public bool IsRevoked { get; set; }

    public DateTime CreatedAt { get; set; }

    // Navigation
    public AppUser? AppUser { get; set; }
}
