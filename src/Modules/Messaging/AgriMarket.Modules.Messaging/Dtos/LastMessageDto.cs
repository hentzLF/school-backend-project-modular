namespace AgriMarket.Modules.Messaging.Dtos;

public sealed class LastMessageDto
{
    public string Content { get; init; } = default!;
    public Guid SenderProfileId { get; init; }
    public DateTime SentAt { get; init; }
}
