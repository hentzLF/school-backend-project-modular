using System.ComponentModel.DataAnnotations;

namespace AgriMarket.Modules.Marketplace.Dtos.Categories;

public sealed class CreateCategoryDto
{
    [Required]
    [MaxLength(100)]
    public string Name { get; init; } = default!;

    [MaxLength(500)]
    public string? Description { get; init; }
}
