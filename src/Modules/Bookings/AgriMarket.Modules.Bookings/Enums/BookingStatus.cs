namespace AgriMarket.Modules.Bookings.Enums;

internal enum BookingStatus
{
    Pending = 1,
    Confirmed = 2,
    InProgress = 3,
    ProviderCompleted = 4,
    ClientConfirmed = 5,
    Archived = 6,
    Cancelled = 7,
    Disputed = 8,
    AwaitingPayment = 9
}
