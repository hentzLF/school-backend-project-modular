using AgriMarket.Modules.Marketplace.Dtos.Locations;
using AgriMarket.Modules.Marketplace.Entities;
using AgriMarket.Shared.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace AgriMarket.Modules.Marketplace.Services;

internal sealed class LocationLookupService(
    [FromKeyedServices("marketplace")] IRepository<County> counties,
    [FromKeyedServices("marketplace")] IRepository<Municipality> municipalities) : ILocationLookupService
{
    public async Task<IReadOnlyList<CountyDto>> GetAllCountiesAsync(CancellationToken ct = default)
    {
        var all = await counties.FindAsync(_ => true, ct);
        return all
            .OrderBy(c => c.Name)
            .Select(c => new CountyDto(c.Id, c.Name, c.EhakCode))
            .ToList();
    }

    public async Task<IReadOnlyList<MunicipalityDto>> GetMunicipalitiesByCountyAsync(Guid countyId, CancellationToken ct = default)
    {
        var filtered = await municipalities.FindAsync(m => m.CountyId == countyId, ct);
        return filtered
            .OrderBy(m => m.Name)
            .Select(m => new MunicipalityDto(m.Id, m.Name, m.EhakCode, m.CountyId))
            .ToList();
    }

    public async Task<bool> CountyExistsAsync(Guid countyId, CancellationToken ct = default)
    {
        return await counties.AnyAsync(c => c.Id == countyId, ct);
    }
}
