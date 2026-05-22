using System.Linq.Expressions;
using AgriMarket.Modules.Marketplace.Entities;
using Microsoft.EntityFrameworkCore;

namespace AgriMarket.Modules.Marketplace.Persistence;

internal sealed class EfListingRepository(MarketplaceDbContext db)
    : EfRepository<ServiceListing>(db), IListingRepository
{
    public async Task<List<ServiceListing>> ListWithSummaryAsync(
        Expression<Func<ServiceListing, bool>>? predicate = null,
        CancellationToken ct = default)
    {
        var query = db.Set<ServiceListing>()
            .AsNoTracking()
            .Include(l => l.ServiceCategory)
            .Include(l => l.Location!).ThenInclude(loc => loc.Municipality!).ThenInclude(m => m.County!)
            .AsQueryable();

        if (predicate is not null)
            query = query.Where(predicate);

        return await query.OrderBy(l => l.Title).ToListAsync(ct);
    }

    public async Task<ServiceListing?> GetWithFullDetailsAsync(Guid id, CancellationToken ct = default)
        => await db.Set<ServiceListing>()
            .AsNoTracking()
            .Include(l => l.ServiceCategory)
            .Include(l => l.Location!).ThenInclude(loc => loc.Municipality!).ThenInclude(m => m.County!)
            .Include(l => l.Availabilities)
            .Include(l => l.ServiceListingEquipments!).ThenInclude(sle => sle.Equipment)
            .FirstOrDefaultAsync(l => l.Id == id, ct);
}
