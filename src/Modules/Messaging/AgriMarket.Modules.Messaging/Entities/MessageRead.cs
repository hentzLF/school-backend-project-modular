namespace AgriMarket.Modules.Messaging.Entities;

internal sealed class MessageRead
{
    public Guid Id { get; set; }

    // FK — cross-module reference, no navigation property
    public Guid UserProfileId { get; set; }

    public DateTime ReadAt { get; set; }

    // FK
    public Guid MessageId { get; set; }

    // Navigation
    public Message? Message { get; set; }
    // UserProfile navigation removed — cross-module reference
}
