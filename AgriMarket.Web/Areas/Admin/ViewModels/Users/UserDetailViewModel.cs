using AgriMarket.Modules.Users.Enums;

namespace AgriMarket.Web.Areas.Admin.ViewModels;

public class UserDetailViewModel
{
    public Guid Id { get; set; }
    public string Email { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
    public bool IsLocked { get; set; }
    public DateTime? LockoutEnd { get; set; }

    public IEnumerable<UserProfileDetailViewModel> Profiles { get; set; } = [];
    public int BookingsCount { get; set; }
    public int ListingsCount { get; set; }
}

public class UserProfileDetailViewModel
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public string? Bio { get; set; }
    public string? AvatarUrl { get; set; }
    public IEnumerable<RoleType> Roles { get; set; } = [];
}
