using AgriMarket.Modules.Messaging.Dtos;
using AgriMarket.Modules.Messaging.Entities;

namespace AgriMarket.Modules.Messaging.Persistence;

/// <summary>
/// Conversation/message queries. Projected DTOs carry profile ids only; the
/// MessagingService fills participant/sender names via <c>IUsersModule</c>.
/// </summary>
internal interface IConversationRepository
{
    Task<Conversation?> FindBetweenParticipantsAsync(Guid profileId1, Guid profileId2, CancellationToken ct = default);

    Task<(List<ConversationSummaryDto> Items, int TotalCount)> ListWithSummariesAsync(
        Guid profileId, int page, int pageSize, CancellationToken ct = default);

    Task<Conversation?> GetWithParticipantsAsync(Guid conversationId, CancellationToken ct = default);

    Task<(List<MessageDto> Items, int TotalCount)> GetMessagesAsync(
        Guid conversationId, Guid callerProfileId, int page, int pageSize, CancellationToken ct = default);

    Task<int> CountUnreadAsync(Guid profileId, CancellationToken ct = default);

    Task<bool> IsParticipantAsync(Guid conversationId, Guid profileId, CancellationToken ct = default);

    Task<List<Guid>> GetUnreadMessageIdsAsync(Guid conversationId, Guid profileId, CancellationToken ct = default);
}
