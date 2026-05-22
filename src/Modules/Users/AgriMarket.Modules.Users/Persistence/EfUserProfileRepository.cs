using AgriMarket.Modules.Users.Entities;
using Microsoft.EntityFrameworkCore;

namespace AgriMarket.Modules.Users.Persistence;

internal sealed class EfUserProfileRepository(UsersDbContext db) : EfRepository<UserProfile>(db), IUserProfileRepository
{
    public async Task<List<UserProfile>> ListWithDetailsAsync(CancellationToken ct = default)
        => await db.Set<UserProfile>()
            .AsNoTracking()
            .Include(p => p.AppUser!)
                .ThenInclude(u => u.Roles)
            .OrderByDescending(p => p.Id)
            .ToListAsync(ct);

    public async Task<UserProfile?> GetByIdWithDetailsAsync(Guid id, CancellationToken ct = default)
        => await db.Set<UserProfile>()
            .AsNoTracking()
            .Include(p => p.AppUser!)
                .ThenInclude(u => u.Roles)
            .FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task<UserProfile?> GetByAppUserIdWithDetailsAsync(Guid appUserId, CancellationToken ct = default)
        => await db.Set<UserProfile>()
            .AsNoTracking()
            .Include(p => p.AppUser)
            .FirstOrDefaultAsync(p => p.AppUserId == appUserId, ct);

    public async Task<(List<UserProfile> Items, int TotalCount)> ListPagedWithDetailsAsync(
        int page, int pageSize, CancellationToken ct = default)
    {
        var baseQuery = db.Set<UserProfile>().AsNoTracking();
        var totalCount = await baseQuery.CountAsync(ct);
        var items = await baseQuery
            .Include(p => p.AppUser!)
                .ThenInclude(u => u.Roles)
            .OrderBy(p => p.FirstName)
            .ThenBy(p => p.LastName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }
}
