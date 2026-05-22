using AgriMarket.Modules.Users.Contracts;
using AgriMarket.Modules.Users.Dtos;
using AgriMarket.Modules.Users.Entities;
using AgriMarket.Modules.Users.Enums;
using AgriMarket.Modules.Users.Persistence;
using AgriMarket.Modules.Bookings.Contracts;
using AgriMarket.Shared.Persistence;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using UserProfileDto = AgriMarket.Modules.Users.Dtos.UserProfileDto;

namespace AgriMarket.Modules.Users.Services;

internal sealed class UserService(
    IAppUserRepository appUsers,
    IUserProfileRepository userProfiles,
    IRepository<UserRole> userRoles,
    [FromKeyedServices("users")] IUnitOfWork uow,
    IBookingsModule bookings,
    IMediator mediator,
    ILogger<UserService> logger) : IUserService
{
    public async Task<IEnumerable<UserProfileDto>> GetAllUsersAsync(CancellationToken ct = default)
    {
        var profiles = await userProfiles.ListWithDetailsAsync(ct);
        return await BuildUserProfileDtosAsync(profiles, includeEmail: true, ct);
    }

    public async Task<UserProfileDto?> GetUserByIdAsync(
        Guid id, Guid? callerUserId = null, bool isAdmin = false, CancellationToken ct = default)
    {
        var profile = await userProfiles.GetByIdWithDetailsAsync(id, ct);
        if (profile is null)
            return null;

        var canSeeEmail = isAdmin || (callerUserId.HasValue && callerUserId.Value == profile.AppUserId);
        return await BuildUserProfileDtoAsync(profile, canSeeEmail ? profile.AppUser?.Email : null, ct);
    }

    public async Task UpdateUserAsync(Guid appUserId, string email, DateTime? lockoutEnd, CancellationToken ct = default)
    {
        var existing = await appUsers.GetByIdAsync(appUserId, ct)
            ?? throw new KeyNotFoundException($"AppUser {appUserId} not found.");
        existing.Email = email;
        existing.LockoutEnd = lockoutEnd;
        await uow.SaveChangesAsync(ct);
    }

    public async Task DeleteUserAsync(Guid id, CancellationToken ct = default)
    {
        var user = await appUsers.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"AppUser {id} not found.");

        var profiles = await userProfiles.FindAsync(p => p.AppUserId == id, ct);
        var profileIds = profiles.Select(p => p.Id).ToList();

        appUsers.Remove(user);
        await uow.SaveChangesAsync(ct);

        logger.LogInformation("AppUser {AppUserId} deleted; cascading to other modules", id);
        await mediator.Publish(new UserDeletedEvent(id, profileIds), ct);
    }

    public async Task LockUserAsync(Guid id, CancellationToken ct = default)
    {
        var user = await appUsers.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"AppUser {id} not found.");
        user.LockoutEnd = DateTime.MaxValue;
        await uow.SaveChangesAsync(ct);
    }

    public async Task UnlockUserAsync(Guid id, CancellationToken ct = default)
    {
        var user = await appUsers.GetByIdAsync(id, ct)
            ?? throw new KeyNotFoundException($"AppUser {id} not found.");
        user.LockoutEnd = null;
        await uow.SaveChangesAsync(ct);
    }

    public async Task<UserProfileDto?> GetProfileByUserIdAsync(Guid appUserId, CancellationToken ct = default)
    {
        var profile = await userProfiles.GetByAppUserIdWithDetailsAsync(appUserId, ct);
        return profile is null ? null : await BuildUserProfileDtoAsync(profile, profile.AppUser?.Email, ct);
    }

    public async Task UpdateProfileAsync(UserProfileDto profile, CancellationToken ct = default)
    {
        var existing = await userProfiles.GetByIdAsync(profile.Id, ct)
            ?? throw new KeyNotFoundException($"UserProfile {profile.Id} not found.");

        existing.FirstName = profile.FirstName;
        existing.LastName = profile.LastName;
        existing.Bio = profile.Bio;
        existing.AvatarUrl = profile.AvatarUrl;
        await uow.SaveChangesAsync(ct);
    }

    public Task<AppUser?> GetByEmailAsync(string email, CancellationToken ct = default)
        => appUsers.GetByEmailWithProfilesAsync(email, ct);

    public async Task<AppUser> CreateUserWithProfileAsync(
        AppUser user, UserProfile profile, RoleType role, CancellationToken ct = default)
    {
        appUsers.Add(user);
        userProfiles.Add(profile);
        userRoles.Add(new UserRole
        {
            Id = Guid.NewGuid(),
            AppUserId = user.Id,
            Role = role
        });
        await uow.SaveChangesAsync(ct);
        return user;
    }

    public async Task<(IEnumerable<UserProfileDto> Items, int TotalCount)> GetAllProfilesAsync(
        int page, int pageSize, CancellationToken ct = default)
    {
        var (profiles, totalCount) = await userProfiles.ListPagedWithDetailsAsync(page, pageSize, ct);
        var dtos = await BuildUserProfileDtosAsync(profiles, includeEmail: false, ct);
        return (dtos, totalCount);
    }

    public async Task<UserProfileDto?> GetProfileByIdAsync(
        Guid id, Guid? callerUserId = null, bool isAdmin = false, CancellationToken ct = default)
    {
        var profile = await userProfiles.GetByIdWithDetailsAsync(id, ct);
        if (profile is null)
            return null;

        var canSeeEmail = isAdmin || (callerUserId.HasValue && callerUserId.Value == profile.AppUserId);
        return await BuildUserProfileDtoAsync(profile, canSeeEmail ? profile.AppUser?.Email : null, ct);
    }

    private async Task<IEnumerable<UserProfileDto>> BuildUserProfileDtosAsync(
        IEnumerable<UserProfile> profiles, bool includeEmail, CancellationToken ct)
    {
        var result = new List<UserProfileDto>();
        foreach (var profile in profiles)
        {
            var email = includeEmail ? profile.AppUser?.Email : null;
            result.Add(await BuildUserProfileDtoAsync(profile, email, ct));
        }
        return result;
    }

    private async Task<UserProfileDto> BuildUserProfileDtoAsync(UserProfile profile, string? email, CancellationToken ct)
    {
        var stats = await bookings.GetProfileRatingAsync(profile.Id, ct);
        return new UserProfileDto
        {
            Id = profile.Id,
            FirstName = profile.FirstName,
            LastName = profile.LastName,
            Bio = profile.Bio,
            AvatarUrl = profile.AvatarUrl,
            AppUserId = profile.AppUserId,
            Email = email,
            CreatedAt = profile.AppUser?.CreatedAt ?? default,
            IsLocked = profile.AppUser?.LockoutEnd > DateTime.UtcNow,
            LockoutEnd = profile.AppUser?.LockoutEnd,
            Roles = profile.AppUser?.Roles?.Select(r => r.Role).ToList() ?? [],
            AverageRating = stats.AverageRating,
            ReviewCount = stats.ReviewCount
        };
    }
}
