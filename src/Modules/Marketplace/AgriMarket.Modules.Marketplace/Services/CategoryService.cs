using AgriMarket.Modules.Marketplace.Entities;
using AgriMarket.Modules.Marketplace.Persistence;
using AgriMarket.Shared.Persistence;
using Microsoft.Extensions.Logging;

namespace AgriMarket.Modules.Marketplace.Services;

internal sealed class CategoryService(
    IRepository<ServiceCategory> categories,
    IRepository<ServiceListing> listings,
    IUnitOfWork uow,
    IQueryMaterializer mat,
    ILogger<CategoryService> logger) : ICategoryService
{
    public async Task<IEnumerable<ServiceCategory>> GetAllAsync()
    {
        return await mat.ToListAsync(categories.Query().OrderBy(c => c.Name));
    }

    public async Task<ServiceCategory?> GetByIdAsync(Guid id)
    {
        return await categories.GetByIdAsync(id);
    }

    public async Task CreateAsync(ServiceCategory category)
    {
        categories.Add(category);
        await uow.SaveChangesAsync();
    }

    public async Task UpdateAsync(ServiceCategory category)
    {
        categories.Update(category);
        await uow.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var category = await categories.GetByIdAsync(id);
        if (category != null)
        {
            categories.Remove(category);
            await uow.SaveChangesAsync();
        }
    }

    public async Task<Dictionary<Guid, int>> GetListingCountsAsync()
    {
        var groups = await mat.ToListAsync(
            listings.Query()
                .GroupBy(l => l.ServiceCategoryId)
                .Select(g => new { CategoryId = g.Key, Count = g.Count() }));
        return groups.ToDictionary(g => g.CategoryId, g => g.Count);
    }

    public async Task<int> GetListingCountAsync(Guid categoryId)
    {
        return await listings.CountAsync(l => l.ServiceCategoryId == categoryId);
    }
}
