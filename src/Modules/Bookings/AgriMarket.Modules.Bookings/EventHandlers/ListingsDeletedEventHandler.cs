using AgriMarket.Modules.Bookings.Entities;
using AgriMarket.Modules.Marketplace.Contracts;
using AgriMarket.Shared.Persistence;
using MediatR;

namespace AgriMarket.Modules.Bookings.EventHandlers;

/// <summary>
/// Cascades a listing deletion into the Bookings module: removes bookings that
/// referenced the deleted listings. Attached payments/reviews cascade at the
/// database.
/// </summary>
internal sealed class ListingsDeletedEventHandler(
    IRepository<Booking> bookings) : INotificationHandler<ListingsDeletedEvent>
{
    public async Task Handle(ListingsDeletedEvent notification, CancellationToken cancellationToken)
    {
        var listingIds = notification.ListingIds.ToList();
        if (listingIds.Count == 0)
            return;

        await bookings.ExecuteDeleteAsync(
            b => listingIds.Contains(b.ServiceListingId),
            cancellationToken);
    }
}
