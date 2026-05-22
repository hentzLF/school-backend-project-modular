using AgriMarket.Modules.Marketplace.Dtos.Locations;

namespace AgriMarket.Modules.Marketplace.Dtos.Listings;

internal sealed class ListingDto
{
    public Guid Id { get; init; }
    public string Title { get; init; } = default!;
    public string? Description { get; init; }
    public decimal PricePerHectare { get; init; }
    public bool IsActive { get; init; }
    public Guid UserProfileId { get; init; }
    public Guid ServiceCategoryId { get; init; }
    public LocationDto? Location { get; init; }
    public string CategoryName { get; init; } = "Unknown";
    public string ProviderName { get; init; } = "Unknown";
    public Guid? ProviderUserId { get; init; }
    public IReadOnlyList<AvailabilityDto> Availabilities { get; init; } = [];
    public IReadOnlyList<ListingEquipmentDto> Equipments { get; init; } = [];
    public double AverageRating { get; init; }
    public int ReviewCount { get; init; }
}
