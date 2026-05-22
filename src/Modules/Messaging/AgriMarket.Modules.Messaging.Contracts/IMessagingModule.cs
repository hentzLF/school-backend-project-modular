namespace AgriMarket.Modules.Messaging.Contracts;

public interface IMessagingModule
{
    Task<int> GetUnreadConversationCountAsync(Guid profileId, CancellationToken ct = default);
}
