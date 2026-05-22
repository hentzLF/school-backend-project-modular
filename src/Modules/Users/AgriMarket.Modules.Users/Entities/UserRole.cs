using AgriMarket.Modules.Users.Enums;

namespace AgriMarket.Modules.Users.Entities;

internal sealed class UserRole
{
    public Guid Id { get; set; }

    public Guid AppUserId { get; set; }

    public RoleType Role { get; set; }

    // Navigation
    public AppUser? AppUser { get; set; }
}
