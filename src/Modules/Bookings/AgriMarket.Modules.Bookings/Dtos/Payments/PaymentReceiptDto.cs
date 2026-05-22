namespace AgriMarket.Modules.Bookings.Dtos.Payments;

internal sealed record PaymentReceiptDto(
    Guid PaymentId,
    Guid BookingId,
    decimal Amount,
    decimal PlatformFee,
    decimal TotalCharged,
    string Method,
    string Status,
    DateTime PaidAt);
