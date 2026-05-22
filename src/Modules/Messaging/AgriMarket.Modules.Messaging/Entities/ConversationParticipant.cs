namespace AgriMarket.Modules.Messaging.Entities;

internal sealed class ConversationParticipant
{
    public Guid UserProfileId { get; set; }

    public DateTime JoinedAt { get; set; }

    // FK
    public Guid ConversationId { get; set; }

    // Navigation
    public Conversation? Conversation { get; set; }
    // UserProfile navigation removed — cross-module reference
}
