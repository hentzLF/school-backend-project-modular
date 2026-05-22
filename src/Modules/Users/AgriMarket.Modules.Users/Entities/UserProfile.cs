namespace AgriMarket.Modules.Users.Entities;

internal sealed class UserProfile
{
    public Guid Id { get; set; }

    public string FirstName { get; set; } = default!;

    public string LastName { get; set; } = default!;

    public string? Bio { get; set; }

    public string? AvatarUrl { get; set; }

    // Foreign Keys
    public Guid AppUserId { get; set; }

    // Navigation
    public AppUser? AppUser { get; set; }
}
