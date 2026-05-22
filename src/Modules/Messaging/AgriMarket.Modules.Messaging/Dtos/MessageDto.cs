namespace AgriMarket.Modules.Messaging.Dtos;

internal sealed class MessageDto
{
    public Guid Id { get; init; }
    public Guid ConversationId { get; init; }
    public Guid SenderProfileId { get; init; }
    public string SenderName { get; set; } = default!;
    public string Content { get; init; } = default!;
    public DateTime SentAt { get; init; }
    public bool IsRead { get; init; }
}
