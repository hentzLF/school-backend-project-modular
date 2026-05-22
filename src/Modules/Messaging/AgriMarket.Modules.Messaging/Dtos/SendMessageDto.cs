using System.ComponentModel.DataAnnotations;

namespace AgriMarket.Modules.Messaging.Dtos;

public sealed class SendMessageDto
{
    [Required]
    public string Content { get; init; } = default!;
}
