using System.Linq.Expressions;
using AgriMarket.Modules.Messaging.Contracts;
using AgriMarket.Modules.Messaging.Dtos;
using AgriMarket.Modules.Messaging.Entities;
using AgriMarket.Modules.Messaging.Persistence;
using AgriMarket.Modules.Messaging.Services;
using AgriMarket.Modules.Users.Contracts;
using AgriMarket.Shared.Exceptions;
using AgriMarket.Shared.Persistence;
using Moq;
using Xunit;

namespace AgriMarket.Tests.Services;

public class MessagingServiceTests
{
    private readonly Mock<IConversationRepository> _conversationRepo = new();
    private readonly Mock<IRepository<Conversation>> _conversations = new();
    private readonly Mock<IRepository<Message>> _messages = new();
    private readonly Mock<IRepository<MessageRead>> _messageReads = new();
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly Mock<IUsersModule> _users = new();
    private readonly Mock<IMessageNotifier> _notifier = new();
    private readonly MessagingService _sut;

    private static readonly Guid CallerId = Guid.NewGuid();
    private static readonly Guid OtherId = Guid.NewGuid();

    public MessagingServiceTests()
    {
        _sut = new MessagingService(
            _conversationRepo.Object,
            _conversations.Object,
            _messages.Object,
            _messageReads.Object,
            _uow.Object,
            _users.Object,
            _notifier.Object);
    }

    private void SetupProfileExists(Guid profileId)
    {
        var dto = new UserProfileDto(profileId, Guid.NewGuid(), "Test", "User", null, null);
        _users
            .Setup(u => u.GetProfilesAsync(
                It.Is<IReadOnlyCollection<Guid>>(ids => ids.Contains(profileId)),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, UserProfileDto> { [profileId] = dto });
    }

    private void SetupBothProfilesExist(Guid profileId1, Guid profileId2)
    {
        var dto1 = new UserProfileDto(profileId1, Guid.NewGuid(), "Alice", "A", null, null);
        var dto2 = new UserProfileDto(profileId2, Guid.NewGuid(), "Bob", "B", null, null);
        _users
            .Setup(u => u.GetProfilesAsync(
                It.IsAny<IReadOnlyCollection<Guid>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, UserProfileDto>
            {
                [profileId1] = dto1,
                [profileId2] = dto2
            });
    }

    private void SetupProfileNotFound(Guid profileId)
    {
        _users
            .Setup(u => u.GetProfilesAsync(
                It.Is<IReadOnlyCollection<Guid>>(ids => ids.Contains(profileId)),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, UserProfileDto>());
    }

    private void SetupConversationExists(Guid conversationId)
    {
        _conversations
            .Setup(r => r.AnyAsync(It.IsAny<Expression<Func<Conversation, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
    }

    private void SetupIsParticipant(Guid conversationId, Guid profileId, bool isParticipant = true)
    {
        _conversationRepo
            .Setup(r => r.IsParticipantAsync(conversationId, profileId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(isParticipant);
    }

    // ===== CreateConversationAsync =====

    [Fact]
    public async Task CreateConversationAsync_ValidCreation_ReturnsConversationDto()
    {
        var dto = new CreateConversationDto { ParticipantProfileIds = [CallerId, OtherId] };
        SetupBothProfilesExist(CallerId, OtherId);
        _conversationRepo
            .Setup(r => r.FindBetweenParticipantsAsync(CallerId, OtherId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Conversation?)null);
        _conversationRepo
            .Setup(r => r.GetWithParticipantsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid id, CancellationToken _) => new Conversation
            {
                Id = id,
                CreatedAt = DateTime.UtcNow,
                Participants =
                [
                    new ConversationParticipant { UserProfileId = CallerId },
                    new ConversationParticipant { UserProfileId = OtherId }
                ]
            });

        var (result, isNew) = await _sut.CreateConversationAsync(CallerId, dto);

        Assert.True(isNew);
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal(2, result.Participants.Count());
        _conversations.Verify(r => r.Add(It.IsAny<Conversation>()), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateConversationAsync_CallerNotInList_ThrowsBusinessRuleException()
    {
        var otherOther = Guid.NewGuid();
        var dto = new CreateConversationDto { ParticipantProfileIds = [OtherId, otherOther] };

        await Assert.ThrowsAsync<BusinessRuleException>(
            () => _sut.CreateConversationAsync(CallerId, dto));
    }

    [Fact]
    public async Task CreateConversationAsync_WrongParticipantCount_ThrowsBusinessRuleException()
    {
        var dto = new CreateConversationDto { ParticipantProfileIds = [CallerId] };

        await Assert.ThrowsAsync<BusinessRuleException>(
            () => _sut.CreateConversationAsync(CallerId, dto));
    }

    [Fact]
    public async Task CreateConversationAsync_TooManyParticipants_ThrowsBusinessRuleException()
    {
        var dto = new CreateConversationDto { ParticipantProfileIds = [CallerId, OtherId, Guid.NewGuid()] };

        await Assert.ThrowsAsync<BusinessRuleException>(
            () => _sut.CreateConversationAsync(CallerId, dto));
    }

    [Fact]
    public async Task CreateConversationAsync_NonExistentProfile_ThrowsKeyNotFoundException()
    {
        var dto = new CreateConversationDto { ParticipantProfileIds = [CallerId, OtherId] };
        SetupProfileNotFound(CallerId);

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _sut.CreateConversationAsync(CallerId, dto));
    }

    [Fact]
    public async Task CreateConversationAsync_DuplicatePrevention_ReturnsExisting()
    {
        var existingId = Guid.NewGuid();
        var dto = new CreateConversationDto { ParticipantProfileIds = [CallerId, OtherId] };
        SetupBothProfilesExist(CallerId, OtherId);
        _conversationRepo
            .Setup(r => r.FindBetweenParticipantsAsync(CallerId, OtherId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Conversation
            {
                Id = existingId,
                CreatedAt = DateTime.UtcNow.AddDays(-1),
                Participants =
                [
                    new ConversationParticipant { UserProfileId = CallerId },
                    new ConversationParticipant { UserProfileId = OtherId }
                ]
            });

        var (result, isNew) = await _sut.CreateConversationAsync(CallerId, dto);

        Assert.False(isNew);
        Assert.Equal(existingId, result.Id);
        _conversations.Verify(r => r.Add(It.IsAny<Conversation>()), Times.Never);
    }

    [Fact]
    public async Task CreateConversationAsync_BookingLinked_AlwaysCreatesNew()
    {
        var bookingId = Guid.NewGuid();
        var dto = new CreateConversationDto
        {
            ParticipantProfileIds = [CallerId, OtherId],
            BookingId = bookingId
        };
        SetupBothProfilesExist(CallerId, OtherId);
        _conversationRepo
            .Setup(r => r.GetWithParticipantsAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid id, CancellationToken _) => new Conversation
            {
                Id = id,
                BookingId = bookingId,
                CreatedAt = DateTime.UtcNow,
                Participants =
                [
                    new ConversationParticipant { UserProfileId = CallerId },
                    new ConversationParticipant { UserProfileId = OtherId }
                ]
            });

        var (result, isNew) = await _sut.CreateConversationAsync(CallerId, dto);

        Assert.True(isNew);
        Assert.Equal(bookingId, result.BookingId);
        _conversations.Verify(r => r.Add(It.IsAny<Conversation>()), Times.Once);
        _conversationRepo.Verify(
            r => r.FindBetweenParticipantsAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    // ===== SendMessageAsync =====

    [Fact]
    public async Task SendMessageAsync_ValidSend_ReturnsMessageDto()
    {
        var conversationId = Guid.NewGuid();
        var dto = new SendMessageDto { Content = "Hello!" };
        SetupConversationExists(conversationId);
        SetupIsParticipant(conversationId, CallerId);
        _users
            .Setup(u => u.GetProfileAsync(CallerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserProfileDto(CallerId, Guid.NewGuid(), "Alice", "A", null, null));

        var result = await _sut.SendMessageAsync(CallerId, conversationId, dto);

        Assert.Equal("Hello!", result.Content);
        Assert.Equal(CallerId, result.SenderProfileId);
        Assert.Equal("Alice A", result.SenderName);
        _messages.Verify(r => r.Add(It.IsAny<Message>()), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SendMessageAsync_NonParticipant_ThrowsUnauthorizedAccessException()
    {
        var conversationId = Guid.NewGuid();
        var dto = new SendMessageDto { Content = "Hello!" };
        SetupConversationExists(conversationId);
        SetupIsParticipant(conversationId, CallerId, false);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _sut.SendMessageAsync(CallerId, conversationId, dto));
    }

    [Fact]
    public async Task SendMessageAsync_EmptyContent_ThrowsBusinessRuleException()
    {
        var conversationId = Guid.NewGuid();
        var dto = new SendMessageDto { Content = "   " };

        await Assert.ThrowsAsync<BusinessRuleException>(
            () => _sut.SendMessageAsync(CallerId, conversationId, dto));
    }

    [Fact]
    public async Task SendMessageAsync_NonExistentConversation_ThrowsKeyNotFoundException()
    {
        var conversationId = Guid.NewGuid();
        var dto = new SendMessageDto { Content = "Hello!" };
        _conversations
            .Setup(r => r.AnyAsync(It.IsAny<Expression<Func<Conversation, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _sut.SendMessageAsync(CallerId, conversationId, dto));
    }

    // ===== GetConversationsAsync =====

    [Fact]
    public async Task GetConversationsAsync_WithConversations_ReturnsPaginatedResult()
    {
        var otherId1 = Guid.NewGuid();
        var otherId2 = Guid.NewGuid();
        var summaries = new List<ConversationSummaryDto>
        {
            new() { Id = Guid.NewGuid(), CreatedAt = DateTime.UtcNow, OtherParticipant = new ParticipantDto { ProfileId = otherId1 } },
            new() { Id = Guid.NewGuid(), CreatedAt = DateTime.UtcNow, OtherParticipant = new ParticipantDto { ProfileId = otherId2 } }
        };
        _conversationRepo
            .Setup(r => r.ListWithSummariesAsync(CallerId, 1, 20, It.IsAny<CancellationToken>()))
            .ReturnsAsync((summaries, 2));
        _users
            .Setup(u => u.GetProfilesAsync(It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, UserProfileDto>());

        var result = await _sut.GetConversationsAsync(CallerId, 1, 20);

        Assert.Equal(2, result.TotalCount);
        Assert.Equal(2, result.Items.Count());
        Assert.Equal(1, result.Page);
        Assert.Equal(20, result.PageSize);
    }

    [Fact]
    public async Task GetConversationsAsync_EmptyResult_ReturnsEmptyPaginatedResult()
    {
        _conversationRepo
            .Setup(r => r.ListWithSummariesAsync(CallerId, 1, 20, It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<ConversationSummaryDto>(), 0));
        _users
            .Setup(u => u.GetProfilesAsync(It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, UserProfileDto>());

        var result = await _sut.GetConversationsAsync(CallerId, 1, 20);

        Assert.Equal(0, result.TotalCount);
        Assert.Empty(result.Items);
    }

    // ===== GetConversationAsync =====

    [Fact]
    public async Task GetConversationAsync_ParticipantAccess_ReturnsConversationWithMessages()
    {
        var conversationId = Guid.NewGuid();
        _conversationRepo
            .Setup(r => r.GetWithParticipantsAsync(conversationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Conversation
            {
                Id = conversationId,
                CreatedAt = DateTime.UtcNow,
                Participants = [new ConversationParticipant { UserProfileId = CallerId }]
            });
        SetupIsParticipant(conversationId, CallerId);
        _conversationRepo
            .Setup(r => r.GetMessagesAsync(conversationId, CallerId, 1, 50, It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<MessageDto> { new() { Id = Guid.NewGuid(), Content = "Hi", SenderProfileId = CallerId } }, 1));
        _users
            .Setup(u => u.GetProfilesAsync(It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<Guid, UserProfileDto>());

        var result = await _sut.GetConversationAsync(CallerId, conversationId, 1, 50);

        Assert.Equal(conversationId, result.Id);
        Assert.Single(result.Messages.Items);
    }

    [Fact]
    public async Task GetConversationAsync_NonParticipant_ThrowsUnauthorizedAccessException()
    {
        var conversationId = Guid.NewGuid();
        _conversationRepo
            .Setup(r => r.GetWithParticipantsAsync(conversationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Conversation
            {
                Id = conversationId,
                CreatedAt = DateTime.UtcNow,
                Participants = []
            });
        SetupIsParticipant(conversationId, CallerId, false);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _sut.GetConversationAsync(CallerId, conversationId, 1, 50));
    }

    [Fact]
    public async Task GetConversationAsync_NonExistentConversation_ThrowsKeyNotFoundException()
    {
        var conversationId = Guid.NewGuid();
        _conversationRepo
            .Setup(r => r.GetWithParticipantsAsync(conversationId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Conversation?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _sut.GetConversationAsync(CallerId, conversationId, 1, 50));
    }

    // ===== MarkAsReadAsync =====

    [Fact]
    public async Task MarkAsReadAsync_FirstRead_CreatesMessageReadRecord()
    {
        var messageId = Guid.NewGuid();
        var conversationId = Guid.NewGuid();
        _messages
            .Setup(r => r.GetByIdAsync(messageId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Message { Id = messageId, ConversationId = conversationId });
        SetupIsParticipant(conversationId, CallerId);
        _messageReads
            .Setup(r => r.AnyAsync(It.IsAny<Expression<Func<MessageRead, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        await _sut.MarkAsReadAsync(CallerId, messageId);

        _messageReads.Verify(r => r.Add(It.Is<MessageRead>(mr =>
            mr.MessageId == messageId && mr.UserProfileId == CallerId)), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task MarkAsReadAsync_IdempotentReRead_DoesNotCreateDuplicate()
    {
        var messageId = Guid.NewGuid();
        var conversationId = Guid.NewGuid();
        _messages
            .Setup(r => r.GetByIdAsync(messageId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Message { Id = messageId, ConversationId = conversationId });
        SetupIsParticipant(conversationId, CallerId);
        _messageReads
            .Setup(r => r.AnyAsync(It.IsAny<Expression<Func<MessageRead, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        await _sut.MarkAsReadAsync(CallerId, messageId);

        _messageReads.Verify(r => r.Add(It.IsAny<MessageRead>()), Times.Never);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task MarkAsReadAsync_NonParticipant_ThrowsUnauthorizedAccessException()
    {
        var messageId = Guid.NewGuid();
        var conversationId = Guid.NewGuid();
        _messages
            .Setup(r => r.GetByIdAsync(messageId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Message { Id = messageId, ConversationId = conversationId });
        SetupIsParticipant(conversationId, CallerId, false);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _sut.MarkAsReadAsync(CallerId, messageId));
    }

    [Fact]
    public async Task MarkAsReadAsync_NonExistentMessage_ThrowsKeyNotFoundException()
    {
        var messageId = Guid.NewGuid();
        _messages
            .Setup(r => r.GetByIdAsync(messageId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Message?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _sut.MarkAsReadAsync(CallerId, messageId));
    }

    // ===== SendMessageAsync — Broadcast =====

    [Fact]
    public async Task SendMessageAsync_Success_BroadcastsReceiveMessage()
    {
        var conversationId = Guid.NewGuid();
        var dto = new SendMessageDto { Content = "Hello!" };
        SetupConversationExists(conversationId);
        SetupIsParticipant(conversationId, CallerId);
        _users
            .Setup(u => u.GetProfileAsync(CallerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserProfileDto(CallerId, Guid.NewGuid(), "Alice", "A", null, null));

        await _sut.SendMessageAsync(CallerId, conversationId, dto);

        _notifier.Verify(n => n.NotifyMessageSentAsync(
            conversationId,
            It.Is<MessageNotificationDto>(m => m.Content == "Hello!" && m.ConversationId == conversationId)),
            Times.Once);
    }

    // ===== MarkAsReadAsync — Broadcast =====

    [Fact]
    public async Task MarkAsReadAsync_NewRead_BroadcastsMessageRead()
    {
        var messageId = Guid.NewGuid();
        var conversationId = Guid.NewGuid();
        _messages
            .Setup(r => r.GetByIdAsync(messageId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Message { Id = messageId, ConversationId = conversationId });
        SetupIsParticipant(conversationId, CallerId);
        _messageReads
            .Setup(r => r.AnyAsync(It.IsAny<Expression<Func<MessageRead, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        await _sut.MarkAsReadAsync(CallerId, messageId);

        _notifier.Verify(n => n.NotifyMessageReadAsync(
            conversationId, messageId, CallerId, It.IsAny<DateTime>()),
            Times.Once);
    }

    [Fact]
    public async Task MarkAsReadAsync_IdempotentReRead_DoesNotBroadcast()
    {
        var messageId = Guid.NewGuid();
        var conversationId = Guid.NewGuid();
        _messages
            .Setup(r => r.GetByIdAsync(messageId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Message { Id = messageId, ConversationId = conversationId });
        SetupIsParticipant(conversationId, CallerId);
        _messageReads
            .Setup(r => r.AnyAsync(It.IsAny<Expression<Func<MessageRead, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        await _sut.MarkAsReadAsync(CallerId, messageId);

        _notifier.Verify(n => n.NotifyMessageReadAsync(
            It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<DateTime>()),
            Times.Never);
    }

    // ===== MarkAllAsReadAsync =====

    [Fact]
    public async Task MarkAllAsReadAsync_WithUnreadMessages_MarksAllAndBroadcasts()
    {
        var conversationId = Guid.NewGuid();
        var msg1 = Guid.NewGuid();
        var msg2 = Guid.NewGuid();
        SetupConversationExists(conversationId);
        SetupIsParticipant(conversationId, CallerId);
        _conversationRepo
            .Setup(r => r.GetUnreadMessageIdsAsync(conversationId, CallerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([msg1, msg2]);

        var count = await _sut.MarkAllAsReadAsync(CallerId, conversationId);

        Assert.Equal(2, count);
        _messageReads.Verify(r => r.Add(It.Is<MessageRead>(mr => mr.MessageId == msg1)), Times.Once);
        _messageReads.Verify(r => r.Add(It.Is<MessageRead>(mr => mr.MessageId == msg2)), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _notifier.Verify(n => n.NotifyMessageReadAsync(conversationId, msg1, CallerId, It.IsAny<DateTime>()), Times.Once);
        _notifier.Verify(n => n.NotifyMessageReadAsync(conversationId, msg2, CallerId, It.IsAny<DateTime>()), Times.Once);
    }

    [Fact]
    public async Task MarkAllAsReadAsync_NoUnreadMessages_ReturnsZero()
    {
        var conversationId = Guid.NewGuid();
        SetupConversationExists(conversationId);
        SetupIsParticipant(conversationId, CallerId);
        _conversationRepo
            .Setup(r => r.GetUnreadMessageIdsAsync(conversationId, CallerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var count = await _sut.MarkAllAsReadAsync(CallerId, conversationId);

        Assert.Equal(0, count);
        _messageReads.Verify(r => r.Add(It.IsAny<MessageRead>()), Times.Never);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task MarkAllAsReadAsync_NonParticipant_ThrowsUnauthorizedAccessException()
    {
        var conversationId = Guid.NewGuid();
        SetupConversationExists(conversationId);
        SetupIsParticipant(conversationId, CallerId, false);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => _sut.MarkAllAsReadAsync(CallerId, conversationId));
    }

    [Fact]
    public async Task MarkAllAsReadAsync_NonExistentConversation_ThrowsKeyNotFoundException()
    {
        var conversationId = Guid.NewGuid();
        _conversations
            .Setup(r => r.AnyAsync(It.IsAny<Expression<Func<Conversation, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _sut.MarkAllAsReadAsync(CallerId, conversationId));
    }

    // ===== GetUnreadCountAsync =====

    [Fact]
    public async Task GetUnreadCountAsync_WithUnread_ReturnsCount()
    {
        _conversationRepo
            .Setup(r => r.CountUnreadAsync(CallerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(5);

        var result = await _sut.GetUnreadCountAsync(CallerId);

        Assert.Equal(5, result.UnreadCount);
    }

    [Fact]
    public async Task GetUnreadCountAsync_AllRead_ReturnsZero()
    {
        _conversationRepo
            .Setup(r => r.CountUnreadAsync(CallerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        var result = await _sut.GetUnreadCountAsync(CallerId);

        Assert.Equal(0, result.UnreadCount);
    }
}
