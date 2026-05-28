using AgriMarket.Modules.Bookings.Entities;
using AgriMarket.Modules.Users.Contracts;
using AgriMarket.Shared.Persistence;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace AgriMarket.Modules.Bookings.EventHandlers;

/// <summary>
/// Cascades a user deletion into the Bookings module: removes the reviews the
/// deleted profiles authored or received, and the bookings they placed as a
/// client. Payments/reviews attached to those bookings cascade at the database.
/// </summary>
internal sealed class UserDeletedEventHandler(
    [FromKeyedServices("bookings")] IRepository<Review> reviews,
    [FromKeyedServices("bookings")] IRepository<Booking> bookings) : INotificationHandler<UserDeletedEvent>
{
    public async Task Handle(UserDeletedEvent notification, CancellationToken cancellationToken)
    {
        var profileIds = notification.ProfileIds.ToList();
        if (profileIds.Count == 0)
            return;

        await reviews.ExecuteDeleteAsync(
            r => profileIds.Contains(r.ReviewerProfileId) || profileIds.Contains(r.ReviewedProfileId),
            cancellationToken);

        await bookings.ExecuteDeleteAsync(
            b => profileIds.Contains(b.ClientProfileId),
            cancellationToken);
    }
}
