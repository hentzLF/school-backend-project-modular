using AgriMarket.Modules.Bookings.Dtos.Bookings;
using AgriMarket.Modules.Bookings.Enums;
using AgriMarket.Modules.Bookings.Services;
using AgriMarket.Shared.Dtos;
using AgriMarket.Shared.Web;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgriMarket.Modules.Bookings.Controllers.Admin;

[ApiController]
[ApiVersion("1")]
[Route("api/v{version:apiVersion}/admin/bookings")]
[Authorize(Policy = "AdminOnly")]
internal sealed class AdminBookingsController(IBookingService bookingService) : ApiControllerBase
{
    private readonly IBookingService _bookingService = bookingService;

    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResponse<BookingDto>), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<IActionResult> GetAll(
        [FromQuery] BookingStatus? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = PaginationDefaults.DefaultPageSize)
    {
        if (pageSize > PaginationDefaults.MaxPageSize) pageSize = PaginationDefaults.MaxPageSize;
        if (page < 1) page = 1;

        var allItems = await _bookingService.GetAllAsync(status);
        var totalCount = allItems.Count();
        var items = allItems.Skip((page - 1) * pageSize).Take(pageSize);

        return Ok(new PaginatedResponse<BookingDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        });
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(BookingDto), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var booking = await _bookingService.GetByIdAsync(id);
        if (booking is null)
            return Problem(statusCode: 404, title: "Not Found", detail: $"Booking {id} not found.");

        return Ok(booking);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(BookingDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateBookingStatusRequest req)
    {
        try
        {
            var booking = await _bookingService.UpdateStatusAsync(id, req.Status);
            return Ok(booking);
        }
        catch (KeyNotFoundException ex)
        {
            return Problem(statusCode: 404, title: "Not Found", detail: ex.Message);
        }
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var booking = await _bookingService.GetByIdAsync(id);
        if (booking is null)
            return Problem(statusCode: 404, title: "Not Found", detail: $"Booking {id} not found.");

        await _bookingService.DeleteAsync(id);
        return NoContent();
    }
}
