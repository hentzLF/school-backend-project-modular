namespace AgriMarket.Modules.Messaging.Contracts;

/// <summary>
/// Public projection of a chat message, passed to <see cref="IMessageNotifier"/>
/// so the real-time transport (SignalR, implemented in the bootstrapper) can
/// push it without referencing Messaging-internal DTOs.
/// </summary>
public sealed record MessageNotificationDto(
    Guid Id,
    Guid ConversationId,
    Guid SenderProfileId,
    string SenderName,
    string Content,
    DateTime SentAt);
