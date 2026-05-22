using AgriMarket.Modules.Messaging.Dtos;

namespace AgriMarket.Modules.Messaging.Services;

internal interface IMessagingService
{
    Task<(ConversationDto Conversation, bool IsNew)> CreateConversationAsync(
        Guid callerProfileId, CreateConversationDto dto, CancellationToken ct = default);

    Task<MessageDto> SendMessageAsync(
        Guid callerProfileId, Guid conversationId, SendMessageDto dto, CancellationToken ct = default);

    Task<PaginatedResponse<ConversationSummaryDto>> GetConversationsAsync(
        Guid callerProfileId, int page, int pageSize, CancellationToken ct = default);

    Task<ConversationDto> GetConversationAsync(
        Guid callerProfileId, Guid conversationId, int page, int pageSize, CancellationToken ct = default);

    Task MarkAsReadAsync(Guid callerProfileId, Guid messageId, CancellationToken ct = default);

    Task<int> MarkAllAsReadAsync(Guid callerProfileId, Guid conversationId, CancellationToken ct = default);

    Task<UnreadCountDto> GetUnreadCountAsync(Guid callerProfileId, CancellationToken ct = default);
}
