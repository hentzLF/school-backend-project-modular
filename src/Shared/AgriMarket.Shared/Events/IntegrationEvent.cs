using MediatR;

namespace AgriMarket.Shared.Events;

public abstract record IntegrationEvent : INotification
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTimeOffset OccurredOn { get; init; } = DateTimeOffset.UtcNow;
}
