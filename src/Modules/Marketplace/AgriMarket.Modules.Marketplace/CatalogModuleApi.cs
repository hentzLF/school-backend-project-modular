using AgriMarket.Modules.Marketplace.Contracts;
using AgriMarket.Modules.Marketplace.Entities;
using AgriMarket.Shared.Persistence;

namespace AgriMarket.Modules.Marketplace;

internal sealed class CatalogModuleApi(
    IRepository<ServiceListing> listings,
    IRepository<Availability> availabilities) : ICatalogModule
{
    public async Task<ListingSummaryDto?> GetListingSummaryAsync(Guid listingId, CancellationToken ct = default)
    {
        var l = await listings.GetByIdAsync(listingId, ct);
        if (l is null)
            return null;

        return new ListingSummaryDto(l.Id, l.Title, l.PricePerHectare, l.IsActive, l.UserProfileId, l.ServiceCategoryId, l.LocationId);
    }

    public async Task<IReadOnlyDictionary<Guid, ListingSummaryDto>> GetListingSummariesAsync(
        IReadOnlyCollection<Guid> listingIds,
        CancellationToken ct = default)
    {
        if (listingIds.Count == 0)
            return new Dictionary<Guid, ListingSummaryDto>();

        var found = await listings.FindAsync(l => listingIds.Contains(l.Id), ct);
        return found.ToDictionary(
            l => l.Id,
            l => new ListingSummaryDto(l.Id, l.Title, l.PricePerHectare, l.IsActive, l.UserProfileId, l.ServiceCategoryId, l.LocationId));
    }

    public async Task<AvailabilityDto?> GetAvailabilityAsync(Guid availabilityId, CancellationToken ct = default)
    {
        var a = await availabilities.GetByIdAsync(availabilityId, ct);
        if (a is null)
            return null;

        return new AvailabilityDto(a.Id, a.ServiceListingId, a.StartTime, a.EndTime, a.IsBooked);
    }
}
