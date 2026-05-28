using AgriMarket.Modules.Messaging.Entities;
using AgriMarket.Modules.Users.Contracts;
using AgriMarket.Shared.Persistence;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace AgriMarket.Modules.Messaging.EventHandlers;

/// <summary>
/// Cascades a user deletion into the Messaging module: removes the deleted
/// profiles' read receipts, sent messages and conversation memberships.
/// </summary>
internal sealed class UserDeletedEventHandler(
    [FromKeyedServices("messaging")] IRepository<MessageRead> messageReads,
    [FromKeyedServices("messaging")] IRepository<Message> messages,
    [FromKeyedServices("messaging")] IRepository<ConversationParticipant> participants)
    : INotificationHandler<UserDeletedEvent>
{
    public async Task Handle(UserDeletedEvent notification, CancellationToken cancellationToken)
    {
        var profileIds = notification.ProfileIds.ToList();
        if (profileIds.Count == 0)
            return;

        await messageReads.ExecuteDeleteAsync(mr => profileIds.Contains(mr.UserProfileId), cancellationToken);
        await messages.ExecuteDeleteAsync(m => profileIds.Contains(m.SenderProfileId), cancellationToken);
        await participants.ExecuteDeleteAsync(cp => profileIds.Contains(cp.UserProfileId), cancellationToken);
    }
}
