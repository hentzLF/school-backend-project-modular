namespace AgriMarket.Modules.Messaging.Dtos;

internal sealed class ConversationSummaryDto
{
    public Guid Id { get; init; }
    public Guid? BookingId { get; init; }
    public ParticipantDto OtherParticipant { get; init; } = default!;
    public LastMessageDto? LastMessage { get; init; }
    public int UnreadCount { get; init; }
    public DateTime CreatedAt { get; init; }
}
