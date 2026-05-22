using System.ComponentModel.DataAnnotations;

namespace AgriMarket.Web.Areas.Client.ViewModels;

internal class LoginViewModel
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = default!;

    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; } = default!;
}
