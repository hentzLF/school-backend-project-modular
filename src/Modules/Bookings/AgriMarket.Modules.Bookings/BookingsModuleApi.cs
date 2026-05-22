using AgriMarket.Modules.Bookings.Contracts;
using AgriMarket.Modules.Bookings.Entities;
using AgriMarket.Shared.Persistence;

namespace AgriMarket.Modules.Bookings;

internal sealed class BookingsModuleApi(IRepository<Booking> bookings) : IBookingsModule
{
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
}
