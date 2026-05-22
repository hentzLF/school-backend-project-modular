using AgriMarket.Modules.Messaging.Contracts;
using AgriMarket.Modules.Messaging.Persistence;

namespace AgriMarket.Modules.Messaging;

internal sealed class MessagingModuleApi(IConversationRepository conversations) : IMessagingModule
{
    public Task<int> GetUnreadConversationCountAsync(Guid profileId, CancellationToken ct = default)
        => conversations.CountUnreadAsync(profileId, ct);
}
