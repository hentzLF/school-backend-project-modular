using AgriMarket.Modules.Users.Enums;

namespace AgriMarket.Modules.Users.Dtos;

/// <summary>
/// Rich, module-internal user/profile projection used by <c>UserService</c> and
/// the Users controllers. Distinct from the lightweight cross-module
/// <see cref="Contracts.UserProfileDto"/>.
/// </summary>
public sealed class UserProfileDto
{
    public Guid Id { get; init; }
    public string FirstName { get; init; } = default!;
    public string LastName { get; init; } = default!;
    public string? Bio { get; init; }
    public string? AvatarUrl { get; init; }
    public Guid AppUserId { get; init; }
    public string? Email { get; init; }
    public DateTime CreatedAt { get; init; }
    public bool IsLocked { get; init; }
    public DateTime? LockoutEnd { get; init; }
    public IEnumerable<RoleType> Roles { get; init; } = [];
    public double AverageRating { get; init; }
    public int ReviewCount { get; init; }
}
