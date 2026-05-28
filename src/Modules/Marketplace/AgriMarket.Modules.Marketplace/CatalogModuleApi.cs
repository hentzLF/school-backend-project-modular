using AgriMarket.Modules.Marketplace.Contracts;
using AgriMarket.Modules.Marketplace.Entities;
using AgriMarket.Shared.Exceptions;
using AgriMarket.Shared.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace AgriMarket.Modules.Marketplace;

internal sealed class CatalogModuleApi(
    [FromKeyedServices("marketplace")] IRepository<ServiceListing> listings,
    [FromKeyedServices("marketplace")] IRepository<Availability> availabilities,
    [FromKeyedServices("marketplace")] IUnitOfWork uow) : ICatalogModule
{
    public async Task<bool> TryReserveAvailabilityAsync(Guid availabilityId, CancellationToken ct = default)
    {
        var availability = await availabilities.GetByIdAsync(availabilityId, ct);
        if (availability is null || availability.IsBooked)
            return false;

        availability.IsBooked = true;
        try
        {
            await uow.SaveChangesAsync(ct);
            return true;
        }
        catch (ConcurrencyException)
        {
            return false;
        }
    }

    public async Task<ListingSummaryDto?> GetListingSummaryAsync(Guid listingId, CancellationToken ct = default)
    {
        var l = await listings.GetByIdAsync(listingId, ct);
        return l is null ? null : ToSummary(l);
    }

    public async Task<IReadOnlyDictionary<Guid, ListingSummaryDto>> GetListingSummariesAsync(
        IReadOnlyCollection<Guid> listingIds,
        CancellationToken ct = default)
    {
        if (listingIds.Count == 0)
            return new Dictionary<Guid, ListingSummaryDto>();

        var found = await listings.FindAsync(l => listingIds.Contains(l.Id), ct);
        return found.ToDictionary(l => l.Id, ToSummary);
    }

    public async Task<AvailabilityDto?> GetAvailabilityAsync(Guid availabilityId, CancellationToken ct = default)
    {
        var a = await availabilities.GetByIdAsync(availabilityId, ct);
        if (a is null)
            return null;

        return new AvailabilityDto(a.Id, a.ServiceListingId, a.StartTime, a.EndTime, a.IsBooked);
    }

    public Task<int> CountListingsAsync(bool? isActive = null, CancellationToken ct = default)
        => isActive is null
            ? listings.CountAsync(_ => true, ct)
            : listings.CountAsync(l => l.IsActive == isActive.Value, ct);

    public async Task<IReadOnlyCollection<ListingSummaryDto>> GetListingsByProviderAsync(
        Guid providerProfileId,
        CancellationToken ct = default)
    {
        var found = await listings.FindAsync(l => l.UserProfileId == providerProfileId, ct);
        return found.Select(ToSummary).ToList();
    }

    private static ListingSummaryDto ToSummary(ServiceListing l) =>
        new(l.Id, l.Title, l.PricePerHectare, l.IsActive, l.UserProfileId, l.ServiceCategoryId, l.LocationId);
}
