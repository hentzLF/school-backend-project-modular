namespace AgriMarket.Web.Areas.Client.ViewModels.Messaging;

internal class ConversationDetailViewModel
{
    public Guid ConversationId { get; set; }
    public string ParticipantName { get; set; } = default!;
    public List<MessageViewModel> Messages { get; set; } = [];
    public SendMessageViewModel SendForm { get; set; } = new();
    public Guid? BookingId { get; set; }
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
}
