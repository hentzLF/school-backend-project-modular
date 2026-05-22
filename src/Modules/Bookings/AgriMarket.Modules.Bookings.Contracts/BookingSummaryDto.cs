namespace AgriMarket.Modules.Bookings.Contracts;

public sealed record BookingSummaryDto(
    Guid Id,
    Guid ServiceListingId,
    Guid ClientProfileId,
    Guid AvailabilityId,
    decimal TotalPrice,
    decimal AreaInHectares,
    string Status,
    DateTime CreatedAt);
