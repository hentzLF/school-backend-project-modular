using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AgriMarket.Web.Areas.Admin.ViewModels;

internal class ListingEditViewModel
{
    public Guid Id { get; set; }

    [Required]
    public string Title { get; set; } = default!;

    public string? Description { get; set; }

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
    public decimal PricePerHectare { get; set; }

    public bool IsActive { get; set; }

    [Required]
    public Guid ServiceCategoryId { get; set; }

    public IEnumerable<SelectListItem> Categories { get; set; } = [];
}
