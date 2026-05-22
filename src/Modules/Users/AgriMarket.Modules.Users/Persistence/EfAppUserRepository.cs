using AgriMarket.Modules.Users.Entities;
using Microsoft.EntityFrameworkCore;

namespace AgriMarket.Modules.Users.Persistence;

internal sealed class EfAppUserRepository(UsersDbContext db) : EfRepository<AppUser>(db), IAppUserRepository
{
    public async Task<AppUser?> GetByEmailWithProfilesAsync(string email, CancellationToken ct = default)
        => await db.Set<AppUser>()
            .Include(u => u.Profile)
            .Include(u => u.Roles)
            .FirstOrDefaultAsync(u => u.Email == email, ct);

    public async Task<AppUser?> GetByIdWithProfilesAsync(Guid id, CancellationToken ct = default)
        => await db.Set<AppUser>()
            .Include(u => u.Profile)
            .Include(u => u.Roles)
            .FirstOrDefaultAsync(u => u.Id == id, ct);
}
