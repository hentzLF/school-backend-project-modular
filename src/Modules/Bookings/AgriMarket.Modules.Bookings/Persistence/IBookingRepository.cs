using System.Linq.Expressions;
using AgriMarket.Modules.Bookings.Entities;
using AgriMarket.Shared.Persistence;

namespace AgriMarket.Modules.Bookings.Persistence;

internal interface IBookingRepository : IRepository<Booking>
{
    Task<List<Booking>> ListWithDetailsAsync(
        Expression<Func<Booking, bool>>? predicate = null,
        int? skip = null,
        int? take = null,
        CancellationToken ct = default);

    Task<int> CountWithDetailsAsync(
        Expression<Func<Booking, bool>>? predicate = null,
        CancellationToken ct = default);

    Task<Booking?> GetByIdWithDetailsAsync(Guid id, CancellationToken ct = default);

    Task<Booking?> GetForUpdateAsync(Guid id, CancellationToken ct = default);

    Task<List<Booking>> ListSummariesByListingAsync(Guid listingId, CancellationToken ct = default);
}
