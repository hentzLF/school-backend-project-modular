namespace AgriMarket.Modules.Marketplace.Contracts;

public sealed record AvailabilityDto(
    Guid Id,
    Guid ServiceListingId,
    DateTime StartTime,
    DateTime EndTime,
    bool IsBooked);
