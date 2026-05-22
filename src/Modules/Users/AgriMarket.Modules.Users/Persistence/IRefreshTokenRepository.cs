using AgriMarket.Modules.Users.Entities;
using AgriMarket.Shared.Persistence;

namespace AgriMarket.Modules.Users.Persistence;

internal interface IRefreshTokenRepository : IRepository<RefreshToken>
{
    Task<RefreshToken?> GetByTokenWithUserAsync(string token, CancellationToken ct = default);
}
