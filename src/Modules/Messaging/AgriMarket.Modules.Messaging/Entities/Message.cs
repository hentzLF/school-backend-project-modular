namespace AgriMarket.Modules.Messaging.Entities;

public sealed class Message
{
    public Guid Id { get; set; }

    public Guid ConversationId { get; set; }

    // FK — cross-module reference, no navigation property
    public Guid SenderProfileId { get; set; }

    public string Content { get; set; } = default!;

    public DateTime SentAt { get; set; }

    // Navigation
    public Conversation? Conversation { get; set; }
    public ICollection<MessageRead>? MessageReads { get; set; }
    // SenderProfile navigation removed — cross-module reference
}
