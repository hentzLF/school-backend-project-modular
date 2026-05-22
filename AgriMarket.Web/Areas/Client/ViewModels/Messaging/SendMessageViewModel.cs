using System.ComponentModel.DataAnnotations;

namespace AgriMarket.Web.Areas.Client.ViewModels.Messaging;

internal class SendMessageViewModel
{
    public Guid ConversationId { get; set; }

    [Required]
    public string Content { get; set; } = default!;
}
