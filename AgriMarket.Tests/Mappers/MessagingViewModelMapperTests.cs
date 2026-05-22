using AgriMarket.Shared.Dtos;
using AgriMarket.Modules.Messaging.Dtos;
using AgriMarket.Web.Mappers;
using FluentAssertions;
using Xunit;

namespace AgriMarket.Tests.Mappers;

public class MessagingViewModelMapperTests
{
    private static readonly Guid CallerId = Guid.NewGuid();
    private static readonly Guid OtherId = Guid.NewGuid();
    private static readonly Guid ConversationId = Guid.NewGuid();

    [Fact]
    public void ToListItem_MapsAllProperties()
    {
        // Arrange
        var dto = new ConversationSummaryDto
        {
            Id = ConversationId,
            BookingId = Guid.NewGuid(),
            OtherParticipant = new ParticipantDto { ProfileId = OtherId, FullName = "John Doe" },
            LastMessage = new LastMessageDto
            {
                Content = "Hello there!",
                SenderProfileId = OtherId,
                SentAt = new DateTime(2026, 1, 15, 10, 30, 0)
            },
            UnreadCount = 3,
            CreatedAt = new DateTime(2026, 1, 15, 9, 0, 0)
        };

        // Act
        var result = dto.ToListItem();

        // Assert
        result.ConversationId.Should().Be(ConversationId);
        result.ParticipantName.Should().Be("John Doe");
        result.LastMessagePreview.Should().Be("Hello there!");
        result.UnreadCount.Should().Be(3);
        result.BookingId.Should().Be(dto.BookingId);
        result.LastActivityAt.Should().Be(new DateTime(2026, 1, 15, 10, 30, 0));
    }

    [Fact]
    public void ToListItem_NullLastMessage_ReturnsNullPreviewAndUsesCreatedAt()
    {
        // Arrange
        var createdAt = new DateTime(2026, 1, 15, 9, 0, 0);
        var dto = new ConversationSummaryDto
        {
            Id = ConversationId,
            OtherParticipant = new ParticipantDto { ProfileId = OtherId, FullName = "John Doe" },
            LastMessage = null,
            UnreadCount = 0,
            CreatedAt = createdAt
        };

        // Act
        var result = dto.ToListItem();

        // Assert
        result.LastMessagePreview.Should().BeNull();
        result.LastActivityAt.Should().Be(createdAt);
    }

    [Fact]
    public void ToListItem_LongMessage_TruncatesPreview()
    {
        // Arrange
        var longMessage = new string('A', 100);
        var dto = new ConversationSummaryDto
        {
            Id = ConversationId,
            OtherParticipant = new ParticipantDto { ProfileId = OtherId, FullName = "John" },
            LastMessage = new LastMessageDto
            {
                Content = longMessage,
                SenderProfileId = OtherId,
                SentAt = DateTime.UtcNow
            },
            CreatedAt = DateTime.UtcNow
        };

        // Act
        var result = dto.ToListItem();

        // Assert
        result.LastMessagePreview.Should().HaveLength(83); // 80 + "..."
        result.LastMessagePreview.Should().EndWith("...");
    }

    [Fact]
    public void ToDetailViewModel_MapsConversationProperties()
    {
        // Arrange
        var bookingId = Guid.NewGuid();
        var dto = new ConversationDto
        {
            Id = ConversationId,
            BookingId = bookingId,
            Participants = new List<ParticipantDto>
            {
                new() { ProfileId = CallerId, FullName = "Caller" },
                new() { ProfileId = OtherId, FullName = "Other User" }
            },
            Messages = new PaginatedResponse<MessageDto>
            {
                Items = new List<MessageDto>
                {
                    new()
                    {
                        Id = Guid.NewGuid(),
                        ConversationId = ConversationId,
                        SenderProfileId = CallerId,
                        SenderName = "Caller",
                        Content = "Hello",
                        SentAt = DateTime.UtcNow,
                        IsRead = true
                    },
                    new()
                    {
                        Id = Guid.NewGuid(),
                        ConversationId = ConversationId,
                        SenderProfileId = OtherId,
                        SenderName = "Other User",
                        Content = "Hi!",
                        SentAt = DateTime.UtcNow,
                        IsRead = false
                    }
                },
                Page = 1,
                PageSize = 20,
                TotalCount = 2
            }
        };

        // Act
        var result = dto.ToDetailViewModel(CallerId);

        // Assert
        result.ConversationId.Should().Be(ConversationId);
        result.ParticipantName.Should().Be("Other User");
        result.BookingId.Should().Be(bookingId);
        result.Messages.Should().HaveCount(2);
        result.Messages[0].IsOwnMessage.Should().BeTrue();
        result.Messages[1].IsOwnMessage.Should().BeFalse();
        result.SendForm.ConversationId.Should().Be(ConversationId);
        result.CurrentPage.Should().Be(1);
        result.TotalPages.Should().Be(1);
    }

    [Fact]
    public void ToDetailViewModel_EmptyMessages_ReturnsSinglePage()
    {
        // Arrange
        var dto = new ConversationDto
        {
            Id = ConversationId,
            Participants = new List<ParticipantDto>
            {
                new() { ProfileId = CallerId, FullName = "Caller" },
                new() { ProfileId = OtherId, FullName = "Other" }
            },
            Messages = new PaginatedResponse<MessageDto>
            {
                Items = new List<MessageDto>(),
                Page = 1,
                PageSize = 20,
                TotalCount = 0
            }
        };

        // Act
        var result = dto.ToDetailViewModel(CallerId);

        // Assert
        result.Messages.Should().BeEmpty();
        result.TotalPages.Should().Be(1);
    }

    [Fact]
    public void ToDetailViewModel_NullBookingId_MapsCorrectly()
    {
        // Arrange
        var dto = new ConversationDto
        {
            Id = ConversationId,
            BookingId = null,
            Participants = new List<ParticipantDto>
            {
                new() { ProfileId = CallerId, FullName = "Caller" },
                new() { ProfileId = OtherId, FullName = "Other" }
            },
            Messages = new PaginatedResponse<MessageDto>
            {
                Items = new List<MessageDto>(),
                Page = 1,
                PageSize = 20,
                TotalCount = 0
            }
        };

        // Act
        var result = dto.ToDetailViewModel(CallerId);

        // Assert
        result.BookingId.Should().BeNull();
    }
}
