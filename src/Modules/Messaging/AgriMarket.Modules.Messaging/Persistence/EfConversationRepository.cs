using AgriMarket.Modules.Messaging.Dtos;
using AgriMarket.Modules.Messaging.Entities;
using Microsoft.EntityFrameworkCore;

namespace AgriMarket.Modules.Messaging.Persistence;

internal sealed class EfConversationRepository(MessagingDbContext db) : IConversationRepository
{
    public async Task<Conversation?> FindBetweenParticipantsAsync(
        Guid profileId1, Guid profileId2, CancellationToken ct = default)
        => await db.Conversations
            .Include(c => c.Participants!)
            .Where(c => c.Participants!.Any(p => p.UserProfileId == profileId1))
            .Where(c => c.Participants!.Any(p => p.UserProfileId == profileId2))
            .FirstOrDefaultAsync(ct);

    public async Task<(List<ConversationSummaryDto> Items, int TotalCount)> ListWithSummariesAsync(
        Guid profileId, int page, int pageSize, CancellationToken ct = default)
    {
        var baseQuery = db.Conversations
            .AsNoTracking()
            .Where(c => c.Participants!.Any(p => p.UserProfileId == profileId));

        var totalCount = await baseQuery.CountAsync(ct);

        var items = await baseQuery
            .OrderByDescending(c => c.Messages!.Max(m => (DateTime?)m.SentAt) ?? c.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new ConversationSummaryDto
            {
                Id = c.Id,
                BookingId = c.BookingId,
                CreatedAt = c.CreatedAt,
                OtherParticipant = c.Participants!
                    .Where(p => p.UserProfileId != profileId)
                    .Select(p => new ParticipantDto { ProfileId = p.UserProfileId, FullName = string.Empty })
                    .First(),
                LastMessage = c.Messages!
                    .OrderByDescending(m => m.SentAt)
                    .Select(m => new LastMessageDto
                    {
                        Content = m.Content,
                        SenderProfileId = m.SenderProfileId,
                        SentAt = m.SentAt
                    })
                    .FirstOrDefault(),
                UnreadCount = c.Messages!
                    .Count(m => m.SenderProfileId != profileId
                        && !m.MessageReads!.Any(mr => mr.UserProfileId == profileId))
            })
            .ToListAsync(ct);

        return (items, totalCount);
    }

    public async Task<Conversation?> GetWithParticipantsAsync(Guid conversationId, CancellationToken ct = default)
        => await db.Conversations
            .Include(c => c.Participants!)
            .FirstOrDefaultAsync(c => c.Id == conversationId, ct);

    public async Task<(List<MessageDto> Items, int TotalCount)> GetMessagesAsync(
        Guid conversationId, Guid callerProfileId, int page, int pageSize, CancellationToken ct = default)
    {
        var baseQuery = db.Messages
            .AsNoTracking()
            .Where(m => m.ConversationId == conversationId);

        var totalCount = await baseQuery.CountAsync(ct);

        var items = await baseQuery
            .OrderByDescending(m => m.SentAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(m => new MessageDto
            {
                Id = m.Id,
                ConversationId = m.ConversationId,
                SenderProfileId = m.SenderProfileId,
                SenderName = string.Empty,
                Content = m.Content,
                SentAt = m.SentAt,
                IsRead = m.MessageReads!.Any(mr => mr.UserProfileId == callerProfileId)
            })
            .ToListAsync(ct);

        items.Reverse();
        return (items, totalCount);
    }

    public async Task<int> CountUnreadAsync(Guid profileId, CancellationToken ct = default)
        => await db.Messages
            .AsNoTracking()
            .Where(m => m.Conversation!.Participants!.Any(p => p.UserProfileId == profileId))
            .Where(m => m.SenderProfileId != profileId)
            .Where(m => !m.MessageReads!.Any(mr => mr.UserProfileId == profileId))
            .CountAsync(ct);

    public async Task<bool> IsParticipantAsync(Guid conversationId, Guid profileId, CancellationToken ct = default)
        => await db.ConversationParticipants
            .AnyAsync(cp => cp.ConversationId == conversationId && cp.UserProfileId == profileId, ct);

    public async Task<List<Guid>> GetUnreadMessageIdsAsync(
        Guid conversationId, Guid profileId, CancellationToken ct = default)
        => await db.Messages
            .AsNoTracking()
            .Where(m => m.ConversationId == conversationId)
            .Where(m => m.SenderProfileId != profileId)
            .Where(m => !m.MessageReads!.Any(mr => mr.UserProfileId == profileId))
            .Select(m => m.Id)
            .ToListAsync(ct);
}
