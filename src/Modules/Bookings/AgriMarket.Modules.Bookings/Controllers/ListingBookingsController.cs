using AgriMarket.Modules.Bookings.Services;
using AgriMarket.Modules.Marketplace.Contracts;
using AgriMarket.Shared.Web;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgriMarket.Modules.Bookings.Controllers;

[ApiController]
[ApiVersion("1")]
[Route("api/v{version:apiVersion}/listings/{listingId:guid}/bookings")]
internal sealed class ListingBookingsController(
    IBookingService bookingService,
    ICatalogModule catalogModule) : ApiControllerBase
{
    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetListingBookings(Guid listingId)
    {
        if (!TryGetProfileId(out var profileId))
            return Problem(statusCode: 401, title: "Unauthorized", detail: "Invalid profile identity.");

        var summary = await catalogModule.GetListingSummaryAsync(listingId);
        if (summary is null)
            return Problem(statusCode: 404, title: "Not Found", detail: $"Listing {listingId} not found.");

        if (summary.UserProfileId != profileId)
            return Problem(statusCode: 403, title: "Forbidden", detail: "You do not own this listing.");

        var bookings = await bookingService.GetByListingAsync(listingId);
        return Ok(bookings);
    }
}
