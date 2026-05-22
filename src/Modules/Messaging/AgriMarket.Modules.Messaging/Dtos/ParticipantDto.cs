namespace AgriMarket.Modules.Messaging.Dtos;

public sealed class ParticipantDto
{
    public Guid ProfileId { get; init; }
    public string FullName { get; set; } = default!;
}
