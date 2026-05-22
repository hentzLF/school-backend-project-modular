using System.ComponentModel.DataAnnotations;

namespace AgriMarket.Modules.Marketplace.Dtos.Categories;

internal sealed class UpdateCategoryDto
{
    [Required]
    [MaxLength(100)]
    public string Name { get; init; } = default!;

    [MaxLength(500)]
    public string? Description { get; init; }
}
