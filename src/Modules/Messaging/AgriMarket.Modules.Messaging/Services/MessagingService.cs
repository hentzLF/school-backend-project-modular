using AgriMarket.Modules.Messaging.Contracts;
using AgriMarket.Modules.Messaging.Dtos;
using AgriMarket.Modules.Messaging.Entities;
using AgriMarket.Modules.Messaging.Persistence;
using AgriMarket.Modules.Users.Contracts;
using AgriMarket.Shared.Exceptions;
using AgriMarket.Shared.Persistence;

namespace AgriMarket.Modules.Messaging.Services;

/// <summary>
/// Conversation/message orchestration. Participant and sender display names are
/// resolved through <see cref="IUsersModule"/> — the Messaging module stores
/// only profile ids.
/// </summary>
internal sealed class MessagingService(
    IConversationRepository conversationRepo,
    IRepository<Conversation> conversations,
    IRepository<Message> messages,
    IRepository<MessageRead> messageReads,
    IUnitOfWork uow,
    IUsersModule users,
    IMessageNotifier notifier) : IMessagingService
{
    private const int DefaultMessagePageSize = 20;

    public async Task<(ConversationDto Conversation, bool IsNew)> CreateConversationAsync(
        Guid callerProfileId, CreateConversationDto dto, CancellationToken ct = default)
    {
        ValidateParticipants(callerProfileId, dto.ParticipantProfileIds);
        await EnsureProfilesExistAsync(dto.ParticipantProfileIds, ct);

        var existing = await FindExistingConversationAsync(dto, ct);
        if (existing is not null)
            return (await ToConversationDtoAsync(existing, ct), false);

        var conversation = BuildConversation(dto);
        conversations.Add(conversation);
        await uow.SaveChangesAsync(ct);

        var saved = await conversationRepo.GetWithParticipantsAsync(conversation.Id, ct);
        return (await ToConversationDtoAsync(saved!, ct), true);
    }

    public async Task<MessageDto> SendMessageAsync(
        Guid callerProfileId, Guid conversationId, SendMessageDto dto, CancellationToken ct = default)
    {
        ValidateMessageContent(dto.Content);
        await EnsureConversationExistsAsync(conversationId, ct);
        await EnsureIsParticipantAsync(conversationId, callerProfileId, ct);

        var sender = await users.GetProfileAsync(callerProfileId, ct)
            ?? throw new KeyNotFoundException($"Sender profile {callerProfileId} not found.");

        var message = new Message
        {
            Id = Guid.NewGuid(),
            ConversationId = conversationId,
            SenderProfileId = callerProfileId,
            Content = dto.Content.Trim(),
            SentAt = DateTime.UtcNow
        };

        messages.Add(message);
        await uow.SaveChangesAsync(ct);

        var result = new MessageDto
        {
            Id = message.Id,
            ConversationId = message.ConversationId,
            SenderProfileId = message.SenderProfileId,
            SenderName = $"{sender.FirstName} {sender.LastName}",
            Content = message.Content,
            SentAt = message.SentAt,
            IsRead = false
        };

        await notifier.NotifyMessageSentAsync(
            conversationId,
            new MessageNotificationDto(
                result.Id, result.ConversationId, result.SenderProfileId,
                result.SenderName, result.Content, result.SentAt));

        return result;
    }

    public async Task<PaginatedResponse<ConversationSummaryDto>> GetConversationsAsync(
        Guid callerProfileId, int page, int pageSize, CancellationToken ct = default)
    {
        var (items, totalCount) = await conversationRepo.ListWithSummariesAsync(callerProfileId, page, pageSize, ct);

        var profiles = await users.GetProfilesAsync(
            items.Select(i => i.OtherParticipant.ProfileId).Distinct().ToList(), ct);

        foreach (var item in items)
            item.OtherParticipant.FullName = ResolveName(item.OtherParticipant.ProfileId, profiles);

        return new PaginatedResponse<ConversationSummaryDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<ConversationDto> GetConversationAsync(
        Guid callerProfileId, Guid conversationId, int page, int pageSize, CancellationToken ct = default)
    {
        var conversation = await conversationRepo.GetWithParticipantsAsync(conversationId, ct)
            ?? throw new KeyNotFoundException($"Conversation {conversationId} not found.");

        await EnsureIsParticipantAsync(conversationId, callerProfileId, ct);

        var (messageItems, totalCount) = await conversationRepo.GetMessagesAsync(
            conversationId, callerProfileId, page, pageSize, ct);

        var profileIds = (conversation.Participants ?? [])
            .Select(p => p.UserProfileId)
            .Concat(messageItems.Select(m => m.SenderProfileId))
            .Distinct()
            .ToList();
        var profiles = await users.GetProfilesAsync(profileIds, ct);

        foreach (var message in messageItems)
            message.SenderName = ResolveName(message.SenderProfileId, profiles);

        return new ConversationDto
        {
            Id = conversation.Id,
            BookingId = conversation.BookingId,
            CreatedAt = conversation.CreatedAt,
            Participants = (conversation.Participants ?? [])
                .Select(p => new ParticipantDto
                {
                    ProfileId = p.UserProfileId,
                    FullName = ResolveName(p.UserProfileId, profiles)
                })
                .ToList(),
            Messages = new PaginatedResponse<MessageDto>
            {
                Items = messageItems,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount
            }
        };
    }

    public async Task MarkAsReadAsync(Guid callerProfileId, Guid messageId, CancellationToken ct = default)
    {
        var message = await messages.GetByIdAsync(messageId, ct)
            ?? throw new KeyNotFoundException($"Message {messageId} not found.");

        await EnsureIsParticipantAsync(message.ConversationId, callerProfileId, ct);

        var alreadyRead = await messageReads.AnyAsync(
            mr => mr.MessageId == messageId && mr.UserProfileId == callerProfileId, ct);
        if (alreadyRead)
            return;

        var readAt = DateTime.UtcNow;
        messageReads.Add(new MessageRead
        {
            Id = Guid.NewGuid(),
            MessageId = messageId,
            UserProfileId = callerProfileId,
            ReadAt = readAt
        });

        await uow.SaveChangesAsync(ct);
        await notifier.NotifyMessageReadAsync(message.ConversationId, messageId, callerProfileId, readAt);
    }

    public async Task<int> MarkAllAsReadAsync(Guid callerProfileId, Guid conversationId, CancellationToken ct = default)
    {
        await EnsureConversationExistsAsync(conversationId, ct);
        await EnsureIsParticipantAsync(conversationId, callerProfileId, ct);

        var unreadIds = await conversationRepo.GetUnreadMessageIdsAsync(conversationId, callerProfileId, ct);
        if (unreadIds.Count == 0)
            return 0;

        var readAt = DateTime.UtcNow;
        foreach (var messageId in unreadIds)
        {
            messageReads.Add(new MessageRead
            {
                Id = Guid.NewGuid(),
                MessageId = messageId,
                UserProfileId = callerProfileId,
                ReadAt = readAt
            });
        }

        await uow.SaveChangesAsync(ct);

        foreach (var messageId in unreadIds)
            await notifier.NotifyMessageReadAsync(conversationId, messageId, callerProfileId, readAt);

        return unreadIds.Count;
    }

    public async Task<UnreadCountDto> GetUnreadCountAsync(Guid callerProfileId, CancellationToken ct = default)
    {
        var count = await conversationRepo.CountUnreadAsync(callerProfileId, ct);
        return new UnreadCountDto { UnreadCount = count };
    }

    private static void ValidateParticipants(Guid callerProfileId, IList<Guid> participantIds)
    {
        if (participantIds.Count != 2)
            throw new BusinessRuleException("A conversation requires exactly 2 participants.");

        if (!participantIds.Contains(callerProfileId))
            throw new BusinessRuleException("You must be one of the conversation participants.");
    }

    private async Task EnsureProfilesExistAsync(IList<Guid> profileIds, CancellationToken ct)
    {
        var found = await users.GetProfilesAsync(profileIds.Distinct().ToList(), ct);
        foreach (var profileId in profileIds)
        {
            if (!found.ContainsKey(profileId))
                throw new KeyNotFoundException($"Profile {profileId} not found.");
        }
    }

    private async Task<Conversation?> FindExistingConversationAsync(CreateConversationDto dto, CancellationToken ct)
    {
        if (dto.BookingId.HasValue)
            return null;

        return await conversationRepo.FindBetweenParticipantsAsync(
            dto.ParticipantProfileIds[0], dto.ParticipantProfileIds[1], ct);
    }

    private static Conversation BuildConversation(CreateConversationDto dto)
    {
        var conversationId = Guid.NewGuid();
        return new Conversation
        {
            Id = conversationId,
            CreatedAt = DateTime.UtcNow,
            BookingId = dto.BookingId,
            Participants = dto.ParticipantProfileIds
                .Select(pid => new ConversationParticipant
                {
                    ConversationId = conversationId,
                    UserProfileId = pid,
                    JoinedAt = DateTime.UtcNow
                })
                .ToList()
        };
    }

    private static void ValidateMessageContent(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            throw new BusinessRuleException("Message content cannot be empty.");
    }

    private async Task EnsureConversationExistsAsync(Guid conversationId, CancellationToken ct)
    {
        var exists = await conversations.AnyAsync(c => c.Id == conversationId, ct);
        if (!exists)
            throw new KeyNotFoundException($"Conversation {conversationId} not found.");
    }

    private async Task EnsureIsParticipantAsync(Guid conversationId, Guid profileId, CancellationToken ct)
    {
        var isParticipant = await conversationRepo.IsParticipantAsync(conversationId, profileId, ct);
        if (!isParticipant)
            throw new UnauthorizedAccessException("You are not a participant of this conversation.");
    }

    private async Task<ConversationDto> ToConversationDtoAsync(Conversation conversation, CancellationToken ct)
    {
        var participants = conversation.Participants ?? [];
        var profiles = await users.GetProfilesAsync(
            participants.Select(p => p.UserProfileId).Distinct().ToList(), ct);

        return new ConversationDto
        {
            Id = conversation.Id,
            BookingId = conversation.BookingId,
            CreatedAt = conversation.CreatedAt,
            Participants = participants
                .Select(p => new ParticipantDto
                {
                    ProfileId = p.UserProfileId,
                    FullName = ResolveName(p.UserProfileId, profiles)
                })
                .ToList(),
            Messages = new PaginatedResponse<MessageDto>
            {
                Items = [],
                Page = 1,
                PageSize = DefaultMessagePageSize,
                TotalCount = 0
            }
        };
    }

    private static string ResolveName(Guid profileId, IReadOnlyDictionary<Guid, UserProfileDto> profiles)
        => profiles.TryGetValue(profileId, out var profile)
            ? $"{profile.FirstName} {profile.LastName}"
            : "Unknown";
}
