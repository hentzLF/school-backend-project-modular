namespace AgriMarket.Modules.Marketplace.Contracts;

public interface ICatalogModule
{
    Task<ListingSummaryDto?> GetListingSummaryAsync(Guid listingId, CancellationToken ct = default);

    Task<IReadOnlyDictionary<Guid, ListingSummaryDto>> GetListingSummariesAsync(
        IReadOnlyCollection<Guid> listingIds,
        CancellationToken ct = default);

    Task<AvailabilityDto?> GetAvailabilityAsync(Guid availabilityId, CancellationToken ct = default);
}
