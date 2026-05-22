using System.ComponentModel.DataAnnotations;

namespace AgriMarket.Modules.Messaging.Dtos;

internal sealed class CreateConversationDto
{
    [Required]
    public IList<Guid> ParticipantProfileIds { get; init; } = [];

    public Guid? BookingId { get; init; }
}
