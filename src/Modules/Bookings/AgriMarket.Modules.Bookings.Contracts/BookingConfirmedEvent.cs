namespace AgriMarket.Modules.Bookings.Contracts;

public sealed record BookingConfirmedEvent(
    Guid BookingId,
    Guid ClientProfileId,
    Guid ProviderProfileId,
    Guid ServiceListingId) : AgriMarket.Shared.Events.IntegrationEvent;
