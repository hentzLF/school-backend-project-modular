namespace AgriMarket.Modules.Bookings.Dtos.Payments;

internal sealed record PaymentHistoryItemDto(
    Guid PaymentId,
    Guid BookingId,
    string ListingTitle,
    decimal Amount,
    decimal PlatformFee,
    string Method,
    string Status,
    DateTime CreatedAt,
    DateTime? ReleasedAt);
