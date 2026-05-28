using AgriMarket.Modules.Bookings.Contracts;
using AgriMarket.Modules.Bookings.Entities;
using AgriMarket.Modules.Bookings.Enums;
using AgriMarket.Shared.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace AgriMarket.Modules.Bookings;

internal sealed class BookingsModuleApi(
    [FromKeyedServices("bookings")] IRepository<Booking> bookings,
    [FromKeyedServices("bookings")] IRepository<Review> reviews,
    IQueryMaterializer materializer) : IBookingsModule
{
    private static readonly BookingStatus[] ActiveStatuses =
    [
        BookingStatus.Pending,
        BookingStatus.AwaitingPayment,
        BookingStatus.Confirmed,
        BookingStatus.InProgress,
        BookingStatus.ProviderCompleted
    ];

    public async Task<BookingSummaryDto?> GetBookingAsync(Guid bookingId, CancellationToken ct = default)
    {
        var b = await bookings.GetByIdAsync(bookingId, ct);
        if (b is null)
            return null;

        return new BookingSummaryDto(
            b.Id,
            b.ServiceListingId,
            b.ClientProfileId,
            b.AvailabilityId,
            b.TotalPrice,
            b.AreaInHectares,
            b.Status.ToString(),
            b.CreatedAt);
    }

    public Task<bool> HasActiveBookingsAsync(Guid listingId, CancellationToken ct = default)
        => bookings.AnyAsync(
            b => b.ServiceListingId == listingId && ActiveStatuses.Contains(b.Status),
            ct);

    public async Task<RatingStatsDto> GetListingRatingAsync(Guid listingId, CancellationToken ct = default)
    {
        var query = reviews.Query().Where(r => r.Booking!.ServiceListingId == listingId);
        return await ComputeStatsAsync(query, ct);
    }

    public async Task<IReadOnlyDictionary<Guid, RatingStatsDto>> GetListingRatingsAsync(
        IReadOnlyCollection<Guid> listingIds,
        CancellationToken ct = default)
    {
        if (listingIds.Count == 0)
            return new Dictionary<Guid, RatingStatsDto>();

        var grouped = reviews.Query()
            .Where(r => listingIds.Contains(r.Booking!.ServiceListingId))
            .GroupBy(r => r.Booking!.ServiceListingId)
            .Select(g => new { ListingId = g.Key, Count = g.Count(), Sum = g.Sum(r => r.Rating) });

        var rows = await materializer.ToListAsync(grouped, ct);
        return rows.ToDictionary(
            r => r.ListingId,
            r => new RatingStatsDto(Math.Round((double)r.Sum / r.Count, 2), r.Count));
    }

    public async Task<RatingStatsDto> GetProfileRatingAsync(Guid profileId, CancellationToken ct = default)
    {
        var query = reviews.Query().Where(r => r.ReviewedProfileId == profileId);
        return await ComputeStatsAsync(query, ct);
    }

    private async Task<RatingStatsDto> ComputeStatsAsync(IQueryable<Review> query, CancellationToken ct)
    {
        var count = await materializer.CountAsync(query, ct);
        if (count == 0)
            return RatingStatsDto.Empty;

        var sum = await materializer.SumAsync(query, r => (decimal?)r.Rating, ct);
        return new RatingStatsDto(Math.Round((double)sum / count, 2), count);
    }
}
