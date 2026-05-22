using AgriMarket.Modules.Users.Enums;

namespace AgriMarket.Web.Areas.Admin.ViewModels;

public class UserListItemViewModel
{
    public Guid Id { get; set; }
    public string Email { get; set; } = default!;
    public int ProfilesCount { get; set; }
    public IEnumerable<RoleType> Roles { get; set; } = [];
    public DateTime CreatedAt { get; set; }
    public bool IsLocked { get; set; }
    public DateTime? LockoutEnd { get; set; }
}
