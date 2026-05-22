using AgriMarket.Modules.Users.Entities;
using AgriMarket.Shared.Persistence;

namespace AgriMarket.Modules.Users.Persistence;

internal interface IAppUserRepository : IRepository<AppUser>
{
    Task<AppUser?> GetByEmailWithProfilesAsync(string email, CancellationToken ct = default);
    Task<AppUser?> GetByIdWithProfilesAsync(Guid id, CancellationToken ct = default);
}
