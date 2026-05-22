using AgriMarket.Modules.Marketplace.Entities;
using AgriMarket.Shared.Persistence;

namespace AgriMarket.Modules.Marketplace.Persistence;

internal interface IAvailabilityRepository : IRepository<Availability>
{
    Task<Availability?> GetWithListingAsync(Guid id, CancellationToken ct = default);
}
