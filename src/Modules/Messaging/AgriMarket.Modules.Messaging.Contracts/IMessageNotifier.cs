namespace AgriMarket.Modules.Messaging.Contracts;

/// <summary>
/// Real-time delivery seam for the Messaging module. The Messaging module core
/// depends on this abstraction; the concrete SignalR-backed implementation
/// lives in the bootstrapper.
/// </summary>
public interface IMessageNotifier
{
    Task NotifyMessageSentAsync(Guid conversationId, MessageNotificationDto message);

    Task NotifyMessageReadAsync(Guid conversationId, Guid messageId, Guid readByProfileId, DateTime readAt);
}
