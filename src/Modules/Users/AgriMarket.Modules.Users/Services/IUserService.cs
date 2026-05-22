using AgriMarket.Modules.Users.Dtos;
using AgriMarket.Modules.Users.Entities;
using AgriMarket.Modules.Users.Enums;

namespace AgriMarket.Modules.Users.Services;

internal interface IUserService
{
    Task<IEnumerable<UserProfileDto>> GetAllUsersAsync(CancellationToken ct = default);
    Task<UserProfileDto?> GetUserByIdAsync(Guid id, Guid? callerUserId = null, bool isAdmin = false, CancellationToken ct = default);
    Task UpdateUserAsync(Guid appUserId, string email, DateTime? lockoutEnd, CancellationToken ct = default);
    Task DeleteUserAsync(Guid id, CancellationToken ct = default);
    Task LockUserAsync(Guid id, CancellationToken ct = default);
    Task UnlockUserAsync(Guid id, CancellationToken ct = default);
    Task<UserProfileDto?> GetProfileByUserIdAsync(Guid appUserId, CancellationToken ct = default);
    Task UpdateProfileAsync(UserProfileDto profile, CancellationToken ct = default);
    Task<AppUser?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<AppUser> CreateUserWithProfileAsync(AppUser user, UserProfile profile, RoleType role, CancellationToken ct = default);
    Task<(IEnumerable<UserProfileDto> Items, int TotalCount)> GetAllProfilesAsync(int page, int pageSize, CancellationToken ct = default);
    Task<UserProfileDto?> GetProfileByIdAsync(Guid id, Guid? callerUserId = null, bool isAdmin = false, CancellationToken ct = default);
}
