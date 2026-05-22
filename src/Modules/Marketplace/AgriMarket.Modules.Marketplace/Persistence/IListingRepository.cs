using System.Linq.Expressions;
using AgriMarket.Modules.Marketplace.Entities;
using AgriMarket.Shared.Persistence;

namespace AgriMarket.Modules.Marketplace.Persistence;

internal interface IListingRepository : IRepository<ServiceListing>
{
    Task<List<ServiceListing>> ListWithSummaryAsync(
        Expression<Func<ServiceListing, bool>>? predicate = null,
        CancellationToken ct = default);

    Task<ServiceListing?> GetWithFullDetailsAsync(Guid id, CancellationToken ct = default);
}
