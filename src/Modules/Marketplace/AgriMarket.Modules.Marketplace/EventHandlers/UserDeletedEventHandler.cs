using AgriMarket.Modules.Marketplace.Contracts;
using AgriMarket.Modules.Marketplace.Entities;
using AgriMarket.Modules.Users.Contracts;
using AgriMarket.Shared.Persistence;
using MediatR;

namespace AgriMarket.Modules.Marketplace.EventHandlers;

/// <summary>
/// Cascades a user deletion into the Marketplace module: removes the deleted
/// providers' equipment and listings (with their locations), then publishes
/// <see cref="ListingsDeletedEvent"/> so the Bookings module can cascade in turn.
/// </summary>
internal sealed class UserDeletedEventHandler(
    IRepository<ServiceListing> listings,
    IRepository<Location> locations,
    IRepository<Equipment> equipment,
    IRepository<ServiceListingEquipment> listingEquipment,
    IMediator mediator) : INotificationHandler<UserDeletedEvent>
{
    public async Task Handle(UserDeletedEvent notification, CancellationToken cancellationToken)
    {
        var profileIds = notification.ProfileIds.ToList();
        if (profileIds.Count == 0)
            return;

        var ownedListings = await listings.FindAsync(l => profileIds.Contains(l.UserProfileId), cancellationToken);
        var listingIds = ownedListings.Select(l => l.Id).ToList();
        var locationIds = ownedListings
            .Where(l => l.LocationId.HasValue)
            .Select(l => l.LocationId!.Value)
            .ToList();

        var equipmentIds = (await equipment.FindAsync(e => profileIds.Contains(e.UserProfileId), cancellationToken))
            .Select(e => e.Id)
            .ToList();

        // Detach equipment from listings, then remove listings (DB cascade
        // clears the remaining join rows) and the now-orphan locations.
        if (equipmentIds.Count > 0)
            await listingEquipment.ExecuteDeleteAsync(sle => equipmentIds.Contains(sle.EquipmentId), cancellationToken);

        await listings.ExecuteDeleteAsync(l => profileIds.Contains(l.UserProfileId), cancellationToken);

        if (locationIds.Count > 0)
            await locations.ExecuteDeleteAsync(loc => locationIds.Contains(loc.Id), cancellationToken);

        await equipment.ExecuteDeleteAsync(e => profileIds.Contains(e.UserProfileId), cancellationToken);

        if (listingIds.Count > 0)
            await mediator.Publish(new ListingsDeletedEvent(listingIds), cancellationToken);
    }
}
