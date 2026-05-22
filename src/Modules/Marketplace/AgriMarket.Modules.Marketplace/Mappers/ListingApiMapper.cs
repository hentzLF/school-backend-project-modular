using AgriMarket.Modules.Marketplace.Dtos.Listings;

namespace AgriMarket.Modules.Marketplace.Mappers;

internal static class ListingApiMapper
{
    public static UpdateListingDto WithRouteId(this UpdateListingDto dto, Guid id)
    {
        return new UpdateListingDto
        {
            Id = id,
            Title = dto.Title,
            Description = dto.Description,
            ServiceCategoryId = dto.ServiceCategoryId,
            PricePerHectare = dto.PricePerHectare,
            IsActive = dto.IsActive,
            Location = dto.Location
        };
    }
}
