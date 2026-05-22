namespace AgriMarket.Modules.Users.Contracts;

public interface IUsersModule
{
    Task<UserProfileDto?> GetProfileAsync(Guid profileId, CancellationToken ct = default);

    Task<IReadOnlyDictionary<Guid, UserProfileDto>> GetProfilesAsync(
        IReadOnlyCollection<Guid> profileIds,
        CancellationToken ct = default);

    /// <summary>Total registered users, optionally only those created on/after a date.</summary>
    Task<int> CountUsersAsync(DateTime? registeredSince = null, CancellationToken ct = default);
}
