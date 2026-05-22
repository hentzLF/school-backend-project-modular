using AgriMarket.Modules.Marketplace.Entities;

namespace AgriMarket.Modules.Marketplace.Services;

internal interface ICategoryService
{
    Task<IEnumerable<ServiceCategory>> GetAllAsync();
    Task<ServiceCategory?> GetByIdAsync(Guid id);
    Task CreateAsync(ServiceCategory category);
    Task UpdateAsync(ServiceCategory category);
    Task DeleteAsync(Guid id);
    Task<Dictionary<Guid, int>> GetListingCountsAsync();
    Task<int> GetListingCountAsync(Guid categoryId);
}
