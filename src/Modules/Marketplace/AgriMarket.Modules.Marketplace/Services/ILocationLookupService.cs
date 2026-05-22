using AgriMarket.Modules.Marketplace.Dtos.Locations;

namespace AgriMarket.Modules.Marketplace.Services;

internal interface ILocationLookupService
{
    Task<IReadOnlyList<CountyDto>> GetAllCountiesAsync(CancellationToken ct = default);
    Task<IReadOnlyList<MunicipalityDto>> GetMunicipalitiesByCountyAsync(Guid countyId, CancellationToken ct = default);
    Task<bool> CountyExistsAsync(Guid countyId, CancellationToken ct = default);
}
