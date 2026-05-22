namespace AgriMarket.Modules.Messaging.Entities;

public sealed class Conversation
{
    public Guid Id { get; set; }

    public DateTime CreatedAt { get; set; }

    // FK — cross-module reference, no navigation property
    public Guid? BookingId { get; set; }

    // Navigation
    public ICollection<ConversationParticipant>? Participants { get; set; }
    public ICollection<Message>? Messages { get; set; }
}
