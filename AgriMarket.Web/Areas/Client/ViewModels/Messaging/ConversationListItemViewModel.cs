namespace AgriMarket.Web.Areas.Client.ViewModels.Messaging;

internal class ConversationListItemViewModel
{
    public Guid ConversationId { get; set; }
    public string ParticipantName { get; set; } = default!;
    public string? LastMessagePreview { get; set; }
    public int UnreadCount { get; set; }
    public Guid? BookingId { get; set; }
    public DateTime LastActivityAt { get; set; }
}
