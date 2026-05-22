using AgriMarket.Modules.Users.Dtos;

namespace AgriMarket.Modules.Users.Mappers;

internal static class UserApiMapper
{
    public static UserProfileDto HideEmail(this UserProfileDto dto)
    {
        return new UserProfileDto
        {
            Id = dto.Id,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Bio = dto.Bio,
            AvatarUrl = dto.AvatarUrl,
            AppUserId = dto.AppUserId,
            Email = null,
            CreatedAt = dto.CreatedAt,
            IsLocked = dto.IsLocked,
            LockoutEnd = dto.LockoutEnd,
            Roles = dto.Roles
        };
    }
}
