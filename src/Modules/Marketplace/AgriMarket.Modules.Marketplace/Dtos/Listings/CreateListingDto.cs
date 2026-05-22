using System.ComponentModel.DataAnnotations;
using AgriMarket.Modules.Marketplace.Dtos.Locations;

namespace AgriMarket.Modules.Marketplace.Dtos.Listings;

internal sealed class CreateListingDto
{
    [Required]
    [MinLength(1)]
    public string Title { get; init; } = default!;

    public string? Description { get; init; }

    [Range(0.01, double.MaxValue)]
    public decimal PricePerHectare { get; init; }

    [Required]
    public Guid ServiceCategoryId { get; init; }

    public CreateLocationDto? Location { get; init; }
}
