namespace AgriMarket.Modules.Messaging.Dtos;

internal sealed class ParticipantDto
{
    public Guid ProfileId { get; init; }
    public string FullName { get; set; } = default!;
}
