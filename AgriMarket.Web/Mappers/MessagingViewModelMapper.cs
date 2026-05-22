using AgriMarket.Modules.Messaging.Dtos;
using AgriMarket.Web.Areas.Client.ViewModels.Messaging;

namespace AgriMarket.Web.Mappers;

public static class MessagingViewModelMapper
{
    private const int MaxPreviewLength = 80;

    public static ConversationListItemViewModel ToListItem(this ConversationSummaryDto dto)
    {
        return new ConversationListItemViewModel
        {
            ConversationId = dto.Id,
            ParticipantName = dto.OtherParticipant.FullName,
            LastMessagePreview = TruncatePreview(dto.LastMessage?.Content),
            UnreadCount = dto.UnreadCount,
            BookingId = dto.BookingId,
            LastActivityAt = dto.LastMessage?.SentAt ?? dto.CreatedAt
        };
    }

    public static ConversationDetailViewModel ToDetailViewModel(
        this ConversationDto dto,
        Guid callerProfileId)
    {
        var otherParticipant = dto.Participants.FirstOrDefault(p => p.ProfileId != callerProfileId);
        var totalPages = dto.Messages.TotalCount > 0
            ? (int)Math.Ceiling((double)dto.Messages.TotalCount / dto.Messages.PageSize)
            : 1;

        return new ConversationDetailViewModel
        {
            ConversationId = dto.Id,
            ParticipantName = otherParticipant?.FullName ?? "Unknown",
            Messages = dto.Messages.Items.Select(m => new MessageViewModel
            {
                Id = m.Id,
                SenderProfileId = m.SenderProfileId,
                SenderName = m.SenderName,
                Content = m.Content,
                SentAt = m.SentAt,
                IsRead = m.IsRead,
                IsOwnMessage = m.SenderProfileId == callerProfileId
            }).ToList(),
            SendForm = new SendMessageViewModel { ConversationId = dto.Id },
            BookingId = dto.BookingId,
            CurrentPage = dto.Messages.Page,
            TotalPages = totalPages
        };
    }

    private static string? TruncatePreview(string? content)
    {
        if (string.IsNullOrEmpty(content))
            return null;

        return content.Length <= MaxPreviewLength
            ? content
            : string.Concat(content.AsSpan(0, MaxPreviewLength), "...");
    }
}
