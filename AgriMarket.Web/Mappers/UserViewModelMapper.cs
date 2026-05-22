using AgriMarket.Modules.Users.Dtos;
using AgriMarket.Web.Areas.Admin.ViewModels;
using AgriMarket.Web.Areas.Client.ViewModels.Profile;

namespace AgriMarket.Web.Mappers;

internal static class UserViewModelMapper
{
    public static ProfileViewModel ToProfileViewModel(this UserProfileDto dto, string role)
    {
        Enum.TryParse<AgriMarket.Modules.Users.Enums.RoleType>(role, out var parsedRole);
        return new ProfileViewModel
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Bio = dto.Bio,
            AvatarUrl = dto.AvatarUrl,
            Role = parsedRole
        };
    }

    public static EditProfileViewModel ToEditProfileViewModel(this UserProfileDto dto)
    {
        return new EditProfileViewModel
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Bio = dto.Bio,
            AvatarUrl = dto.AvatarUrl
        };
    }

    public static UserListItemViewModel ToUserListItem(this UserProfileDto dto)
    {
        return new UserListItemViewModel
        {
            Id = dto.Id,
            Email = dto.Email ?? string.Empty,
            ProfilesCount = 1,
            Roles = dto.Roles,
            CreatedAt = dto.CreatedAt,
            IsLocked = dto.IsLocked,
            LockoutEnd = dto.LockoutEnd
        };
    }
}
