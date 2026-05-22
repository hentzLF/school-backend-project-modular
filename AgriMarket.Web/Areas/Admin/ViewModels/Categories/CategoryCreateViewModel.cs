using System.ComponentModel.DataAnnotations;

namespace AgriMarket.Web.Areas.Admin.ViewModels;

internal class CategoryCreateViewModel
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = default!;

    public string? Description { get; set; }
}
