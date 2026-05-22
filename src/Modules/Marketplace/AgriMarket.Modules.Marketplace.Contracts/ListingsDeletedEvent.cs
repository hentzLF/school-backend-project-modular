using AgriMarket.Shared.Events;

namespace AgriMarket.Modules.Marketplace.Contracts;

/// <summary>
/// Raised after service listings have been removed from the Marketplace
/// module. The Bookings module handles this to cascade-delete bookings that
/// referenced the removed listings.
/// </summary>
public sealed record ListingsDeletedEvent(
    IReadOnlyCollection<Guid> ListingIds) : IntegrationEvent;
