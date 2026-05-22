using AgriMarket.Modules.Messaging.Contracts;

namespace AgriMarket.Web.Services;

public class NoOpMessageNotifier : IMessageNotifier
{
    public Task NotifyMessageSentAsync(Guid conversationId, MessageNotificationDto message) => Task.CompletedTask;
    public Task NotifyMessageReadAsync(Guid conversationId, Guid messageId, Guid readByProfileId, DateTime readAt) => Task.CompletedTask;
}
