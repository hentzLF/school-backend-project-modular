using AgriMarket.Modules.Users.Contracts;
using AgriMarket.Modules.Users.Entities;
using AgriMarket.Modules.Users.Persistence;

namespace AgriMarket.Modules.Users;

internal sealed class UsersModuleApi(IUserProfileRepository profiles) : IUsersModule
{
    public async Task<UserProfileDto?> GetProfileAsync(Guid profileId, CancellationToken ct = default)
    {
        var profile = await profiles.GetByIdAsync(profileId, ct);
        return profile is null ? null : MapToDto(profile);
    }

    public async Task<IReadOnlyDictionary<Guid, UserProfileDto>> GetProfilesAsync(
        IReadOnlyCollection<Guid> profileIds,
        CancellationToken ct = default)
    {
        if (profileIds.Count == 0)
            return new Dictionary<Guid, UserProfileDto>();

        var found = await profiles.FindAsync(p => profileIds.Contains(p.Id), ct);
        return found.ToDictionary(p => p.Id, MapToDto);
    }

    private static UserProfileDto MapToDto(UserProfile profile) =>
        new(
            profile.Id,
            profile.AppUserId,
            profile.FirstName,
            profile.LastName,
            profile.Bio,
            profile.AvatarUrl);
}
