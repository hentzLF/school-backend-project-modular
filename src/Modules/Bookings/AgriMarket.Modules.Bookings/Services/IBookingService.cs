using AgriMarket.Modules.Bookings.Dtos.Bookings;
using AgriMarket.Modules.Bookings.Enums;

namespace AgriMarket.Modules.Bookings.Services;

internal interface IBookingService
{
    Task<IEnumerable<BookingDto>> GetAllAsync(BookingStatus? status = null, CancellationToken ct = default);
    Task<BookingDto?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IEnumerable<BookingDto>> GetByClientAsync(Guid clientProfileId, CancellationToken ct = default);
    Task<IEnumerable<BookingDto>> GetByProviderAsync(Guid providerProfileId, CancellationToken ct = default);

    /// <param name="clientProfileId">UserProfile id of the booking client.</param>
    Task<BookingDto> CreateAsync(Guid clientProfileId, CreateBookingDto dto, CancellationToken ct = default);

    /// <param name="callerProfileId">UserProfile id of the caller, for transition authorization. Null skips the check (admin).</param>
    Task<BookingDto> UpdateStatusAsync(Guid id, BookingStatus status, Guid? callerProfileId = null, CancellationToken ct = default);

    Task DeleteAsync(Guid id, CancellationToken ct = default);
    Task<int> GetCountByListingAsync(Guid listingId, CancellationToken ct = default);
    Task<bool> HasActiveBookingsAsync(Guid listingId, CancellationToken ct = default);
    Task<IEnumerable<BookingSummaryDto>> GetByListingAsync(Guid listingId, CancellationToken ct = default);
    Task<(IEnumerable<BookingDto> Items, int TotalCount)> GetAllForProfileAsync(Guid profileId, int page, int pageSize, CancellationToken ct = default);
}
