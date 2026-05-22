using AgriMarket.Modules.Messaging.Contracts;
using AgriMarket.Modules.Messaging.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace AgriMarket.Api.Hubs;

/// <summary>
/// SignalR-backed implementation of the Messaging module's
/// <see cref="IMessageNotifier"/> seam. Lives in the bootstrapper because it
/// owns the SignalR transport.
/// </summary>
internal sealed class SignalRMessageNotifier(IHubContext<MessageHub> hubContext) : IMessageNotifier
{
    public async Task NotifyMessageSentAsync(Guid conversationId, MessageNotificationDto message)
    {
        await hubContext.Clients
            .Group(MessageHub.GroupName(conversationId))
            .SendAsync("ReceiveMessage", message);
    }

    public async Task NotifyMessageReadAsync(
        Guid conversationId, Guid messageId, Guid readByProfileId, DateTime readAt)
    {
        await hubContext.Clients
            .Group(MessageHub.GroupName(conversationId))
            .SendAsync("MessageRead", new { messageId, readByProfileId, readAt });
    }
}
