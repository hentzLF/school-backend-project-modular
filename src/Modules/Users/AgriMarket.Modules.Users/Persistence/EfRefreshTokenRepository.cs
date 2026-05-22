using AgriMarket.Modules.Users.Entities;
using Microsoft.EntityFrameworkCore;

namespace AgriMarket.Modules.Users.Persistence;

internal sealed class EfRefreshTokenRepository(UsersDbContext db) : EfRepository<RefreshToken>(db), IRefreshTokenRepository
{
    public async Task<RefreshToken?> GetByTokenWithUserAsync(string token, CancellationToken ct = default)
        => await db.Set<RefreshToken>()
            .Include(rt => rt.AppUser!)
                .ThenInclude(u => u.Profile)
            .Include(rt => rt.AppUser!)
                .ThenInclude(u => u.Roles)
            .FirstOrDefaultAsync(rt => rt.Token == token, ct);
}
