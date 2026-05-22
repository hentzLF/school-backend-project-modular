namespace AgriMarket.Modules.Bookings.Contracts;

public interface IBookingsModule
{
    Task<BookingSummaryDto?> GetBookingAsync(Guid bookingId, CancellationToken ct = default);

    /// <summary>
    /// True when the listing has bookings in a non-terminal state. Used by the
    /// Marketplace module to guard listing deletion.
    /// </summary>
    Task<bool> HasActiveBookingsAsync(Guid listingId, CancellationToken ct = default);

    /// <summary>Aggregate review rating for a single service listing.</summary>
    Task<RatingStatsDto> GetListingRatingAsync(Guid listingId, CancellationToken ct = default);

    /// <summary>Aggregate review rating for several service listings at once.</summary>
    Task<IReadOnlyDictionary<Guid, RatingStatsDto>> GetListingRatingsAsync(
        IReadOnlyCollection<Guid> listingIds,
        CancellationToken ct = default);

    /// <summary>Aggregate review rating received by a user profile (as a provider).</summary>
    Task<RatingStatsDto> GetProfileRatingAsync(Guid profileId, CancellationToken ct = default);
}
