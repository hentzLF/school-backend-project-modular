using AgriMarket.Modules.Users.Entities;
using AgriMarket.Shared.Persistence;

namespace AgriMarket.Modules.Users.Persistence;

internal interface IUserProfileRepository : IRepository<UserProfile>
{
    Task<List<UserProfile>> ListWithDetailsAsync(CancellationToken ct = default);
    Task<UserProfile?> GetByIdWithDetailsAsync(Guid id, CancellationToken ct = default);
    Task<UserProfile?> GetByAppUserIdWithDetailsAsync(Guid appUserId, CancellationToken ct = default);
    Task<(List<UserProfile> Items, int TotalCount)> ListPagedWithDetailsAsync(int page, int pageSize, CancellationToken ct = default);
}
