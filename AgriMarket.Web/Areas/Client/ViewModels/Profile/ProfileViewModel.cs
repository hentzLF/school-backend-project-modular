using AgriMarket.Modules.Users.Enums;

namespace AgriMarket.Web.Areas.Client.ViewModels.Profile;

internal class ProfileViewModel
{
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public string? Bio { get; set; }
    public string? AvatarUrl { get; set; }
    public RoleType Role { get; set; }
}
