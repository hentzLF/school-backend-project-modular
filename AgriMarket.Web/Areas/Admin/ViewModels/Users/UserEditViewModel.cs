using System.ComponentModel.DataAnnotations;

namespace AgriMarket.Web.Areas.Admin.ViewModels;

internal class UserEditViewModel
{
    public Guid Id { get; set; }

    [Required]
    [EmailAddress]
    public string Email { get; set; } = default!;

    public DateTime? LockoutEnd { get; set; }
}
