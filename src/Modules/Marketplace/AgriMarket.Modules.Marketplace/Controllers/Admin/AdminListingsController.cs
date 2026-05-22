using AgriMarket.Modules.Marketplace.Dtos.Listings;
using AgriMarket.Modules.Marketplace.Mappers;
using AgriMarket.Modules.Marketplace.Services;
using AgriMarket.Shared.Dtos;
using AgriMarket.Shared.Exceptions;
using AgriMarket.Shared.Web;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgriMarket.Modules.Marketplace.Controllers.Admin;

[ApiController]
[ApiVersion("1")]
[Route("api/v{version:apiVersion}/admin/listings")]
[Authorize(Policy = "AdminOnly")]
internal sealed class AdminListingsController(IListingService listingService) : ApiControllerBase
{
    private readonly IListingService _listingService = listingService;

    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResponse<ListingSummaryDto>), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        if (pageSize > 100) pageSize = 100;
        if (page < 1) page = 1;

        var allItems = await _listingService.GetAllAsync();
        var totalCount = allItems.Count();
        var items = allItems.Skip((page - 1) * pageSize).Take(pageSize);

        return Ok(new PaginatedResponse<ListingSummaryDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        });
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ListingDto), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var listing = await _listingService.GetByIdAsync(id);
        if (listing is null)
            return Problem(statusCode: 404, title: "Not Found", detail: $"ServiceListing {id} not found.");

        return Ok(listing);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ListingDto), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    [ProducesResponseType(422)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateListingDto req)
    {
        try
        {
            var listing = await _listingService.AdminUpdateAsync(req.WithRouteId(id));
            return Ok(listing);
        }
        catch (KeyNotFoundException ex)
        {
            return Problem(statusCode: 404, title: "Not Found", detail: ex.Message);
        }
        catch (BusinessRuleException ex)
        {
            return Problem(statusCode: 422, title: "Unprocessable Entity", detail: ex.Message);
        }
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    [ProducesResponseType(422)]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            await _listingService.AdminDeleteAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return Problem(statusCode: 404, title: "Not Found", detail: ex.Message);
        }
        catch (BusinessRuleException ex)
        {
            return Problem(statusCode: 422, title: "Unprocessable Entity", detail: ex.Message);
        }
    }
}
