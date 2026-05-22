namespace AgriMarket.Modules.Users.Contracts;

public sealed record UserProfileDto(
    Guid Id,
    Guid AppUserId,
    string FirstName,
    string LastName,
    string? Bio,
    string? AvatarUrl);
