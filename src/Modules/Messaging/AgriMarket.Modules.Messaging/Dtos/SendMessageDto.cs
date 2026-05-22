using System.ComponentModel.DataAnnotations;

namespace AgriMarket.Modules.Messaging.Dtos;

internal sealed class SendMessageDto
{
    [Required]
    public string Content { get; init; } = default!;
}
