using AgriMarket.Shared.Dtos;

namespace AgriMarket.Modules.Messaging.Dtos;

internal sealed class ConversationDto
{
    public Guid Id { get; init; }
    public Guid? BookingId { get; init; }
    public DateTime CreatedAt { get; init; }
    public IEnumerable<ParticipantDto> Participants { get; init; } = [];
    public PaginatedResponse<MessageDto> Messages { get; init; } = default!;
}
