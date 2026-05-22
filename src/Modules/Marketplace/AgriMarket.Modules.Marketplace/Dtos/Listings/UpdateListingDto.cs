using System.ComponentModel.DataAnnotations;
using AgriMarket.Modules.Marketplace.Dtos.Locations;

namespace AgriMarket.Modules.Marketplace.Dtos.Listings;

public sealed class UpdateListingDto
{
    [Required]
    public Guid Id { get; init; }

    [Required]
    [MinLength(1)]
    public string Title { get; init; } = default!;

    public string? Description { get; init; }

    [Range(0.01, double.MaxValue)]
    public decimal PricePerHectare { get; init; }

    public bool IsActive { get; init; }

    [Required]
    public Guid ServiceCategoryId { get; init; }

    public UpdateLocationDto? Location { get; init; }
}
