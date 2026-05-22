using AgriMarket.Shared.Events;

namespace AgriMarket.Modules.Users.Contracts;

/// <summary>
/// Raised after an application user (and their profiles) has been removed from
/// the Users module. Other modules handle this to cascade-delete the data they
/// own that referenced the deleted profiles.
/// </summary>
public sealed record UserDeletedEvent(
    Guid AppUserId,
    IReadOnlyCollection<Guid> ProfileIds) : IntegrationEvent;
