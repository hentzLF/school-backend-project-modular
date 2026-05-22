namespace AgriMarket.Web.Areas.Client.ViewModels.Messaging;

internal class MessageViewModel
{
    public Guid Id { get; set; }
    public Guid SenderProfileId { get; set; }
    public string SenderName { get; set; } = default!;
    public string Content { get; set; } = default!;
    public DateTime SentAt { get; set; }
    public bool IsRead { get; set; }
    public bool IsOwnMessage { get; set; }
}
