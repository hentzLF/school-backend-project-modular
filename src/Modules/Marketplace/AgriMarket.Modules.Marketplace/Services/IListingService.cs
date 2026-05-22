using AgriMarket.Modules.Marketplace.Dtos.Listings;

namespace AgriMarket.Modules.Marketplace.Services;

internal interface IListingService
{
    Task<IEnumerable<ListingSummaryDto>> GetAllAsync(CancellationToken ct = default);
    Task<ListingDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IEnumerable<ListingSummaryDto>> GetByProviderAsync(Guid providerProfileId, CancellationToken ct = default);
    Task<IEnumerable<ListingSummaryDto>> GetActiveListingsAsync(CancellationToken ct = default);
    Task<ListingDto> CreateAsync(Guid userId, CreateListingDto dto, CancellationToken ct = default);
    Task<ListingDto> UpdateAsync(Guid userId, UpdateListingDto dto, CancellationToken ct = default);
    Task DeleteAsync(Guid userId, Guid listingId, CancellationToken ct = default);
    Task ToggleActiveAsync(Guid userId, Guid listingId, CancellationToken ct = default);
    Task<AvailabilityDto> AddAvailabilityAsync(Guid userId, CreateAvailabilityDto dto, CancellationToken ct = default);
    Task DeleteAvailabilityAsync(Guid userId, Guid availabilityId, CancellationToken ct = default);
    Task<AvailabilityDto?> GetAvailabilityByIdAsync(Guid id, CancellationToken ct = default);
    Task<ListingDto> AdminUpdateAsync(UpdateListingDto dto, CancellationToken ct = default);
    Task AdminDeleteAsync(Guid listingId, CancellationToken ct = default);
    Task AdminToggleActiveAsync(Guid listingId, CancellationToken ct = default);
}
