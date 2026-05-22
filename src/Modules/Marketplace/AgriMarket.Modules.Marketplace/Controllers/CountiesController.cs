using AgriMarket.Modules.Marketplace.Dtos.Locations;
using AgriMarket.Modules.Marketplace.Services;
using AgriMarket.Shared.Web;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgriMarket.Modules.Marketplace.Controllers;

[ApiController]
[ApiVersion("1")]
[Route("api/v{version:apiVersion}/counties")]
[AllowAnonymous]
internal sealed class CountiesController(ILocationLookupService locationLookupService) : ApiControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<CountyDto>), 200)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var counties = await locationLookupService.GetAllCountiesAsync(ct);
        return Ok(counties);
    }

    [HttpGet("{countyId:guid}/municipalities")]
    [ProducesResponseType(typeof(IReadOnlyList<MunicipalityDto>), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetMunicipalities(Guid countyId, CancellationToken ct)
    {
        var exists = await locationLookupService.CountyExistsAsync(countyId, ct);
        if (!exists)
            return NotFound();

        var municipalities = await locationLookupService.GetMunicipalitiesByCountyAsync(countyId, ct);
        return Ok(municipalities);
    }
}
