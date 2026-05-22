namespace AgriMarket.Modules.Marketplace.Contracts;

public interface ICatalogModule
{
    Task<ListingSummaryDto?> GetListingSummaryAsync(Guid listingId, CancellationToken ct = default);

    Task<IReadOnlyDictionary<Guid, ListingSummaryDto>> GetListingSummariesAsync(
        IReadOnlyCollection<Guid> listingIds,
        CancellationToken ct = default);

    Task<AvailabilityDto?> GetAvailabilityAsync(Guid availabilityId, CancellationToken ct = default);

    /// <summary>Total listing count, optionally filtered by active state.</summary>
    Task<int> CountListingsAsync(bool? isActive = null, CancellationToken ct = default);

    /// <summary>All listing summaries owned by a single provider profile.</summary>
    Task<IReadOnlyCollection<ListingSummaryDto>> GetListingsByProviderAsync(
        Guid providerProfileId,
        CancellationToken ct = default);

    /// <summary>
    /// Atomically marks an availability slot as booked. Returns false when the
    /// slot does not exist or was already booked (lost a concurrency race).
    /// </summary>
    Task<bool> TryReserveAvailabilityAsync(Guid availabilityId, CancellationToken ct = default);
}
