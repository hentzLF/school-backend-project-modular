using System.ComponentModel.DataAnnotations;

namespace AgriMarket.Web.Areas.Client.ViewModels.Profile;

internal class EditProfileViewModel
{
    [Required]
    [MaxLength(100)]
    public string FirstName { get; set; } = default!;

    [Required]
    [MaxLength(100)]
    public string LastName { get; set; } = default!;

    [MaxLength(500)]
    public string? Bio { get; set; }

    [MaxLength(500)]
    public string? AvatarUrl { get; set; }
}
