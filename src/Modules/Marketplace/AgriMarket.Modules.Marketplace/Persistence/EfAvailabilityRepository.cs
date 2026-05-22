using AgriMarket.Modules.Marketplace.Entities;
using Microsoft.EntityFrameworkCore;

namespace AgriMarket.Modules.Marketplace.Persistence;

internal sealed class EfAvailabilityRepository(MarketplaceDbContext db)
    : EfRepository<Availability>(db), IAvailabilityRepository
{
    public async Task<Availability?> GetWithListingAsync(Guid id, CancellationToken ct = default)
        => await db.Set<Availability>()
            .Include(a => a.ServiceListing)
            .FirstOrDefaultAsync(a => a.Id == id, ct);
}
