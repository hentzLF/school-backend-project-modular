namespace AgriMarket.Modules.Bookings.Contracts;

public interface IBookingsModule
{
    Task<BookingSummaryDto?> GetBookingAsync(Guid bookingId, CancellationToken ct = default);
}
