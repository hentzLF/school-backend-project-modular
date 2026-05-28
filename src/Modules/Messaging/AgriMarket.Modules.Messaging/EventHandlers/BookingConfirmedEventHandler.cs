using AgriMarket.Modules.Bookings.Contracts;
using AgriMarket.Modules.Messaging.Entities;
using AgriMarket.Modules.Messaging.Persistence;
using AgriMarket.Shared.Persistence;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace AgriMarket.Modules.Messaging.EventHandlers;

/// <summary>
/// When a booking is confirmed, opens a conversation between the client and the
/// provider (idempotent — skipped if one already exists for the booking).
/// </summary>
internal sealed class BookingConfirmedEventHandler(
    [FromKeyedServices("messaging")] IRepository<Conversation> conversations,
    [FromKeyedServices("messaging")] IUnitOfWork uow) : INotificationHandler<BookingConfirmedEvent>
{
    public async Task Handle(BookingConfirmedEvent notification, CancellationToken cancellationToken)
    {
        if (notification.ClientProfileId == Guid.Empty
            || notification.ProviderProfileId == Guid.Empty
            || notification.ClientProfileId == notification.ProviderProfileId)
        {
            return;
        }

        var alreadyExists = await conversations.AnyAsync(
            c => c.BookingId == notification.BookingId, cancellationToken);
        if (alreadyExists)
            return;

        var conversationId = Guid.NewGuid();
        var now = DateTime.UtcNow;
        conversations.Add(new Conversation
        {
            Id = conversationId,
            CreatedAt = now,
            BookingId = notification.BookingId,
            Participants =
            [
                new ConversationParticipant
                {
                    ConversationId = conversationId,
                    UserProfileId = notification.ClientProfileId,
                    JoinedAt = now
                },
                new ConversationParticipant
                {
                    ConversationId = conversationId,
                    UserProfileId = notification.ProviderProfileId,
                    JoinedAt = now
                }
            ]
        });

        await uow.SaveChangesAsync(cancellationToken);
    }
}
