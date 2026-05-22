using AgriMarket.Modules.Bookings.Dtos.Reviews;
using AgriMarket.Modules.Bookings.Mappers;
using AgriMarket.Modules.Bookings.Services;
using AgriMarket.Shared.Dtos;
using AgriMarket.Shared.Exceptions;
using AgriMarket.Shared.Web;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgriMarket.Modules.Bookings.Controllers;

[ApiController]
[ApiVersion("1")]
[Route("api/v{version:apiVersion}/reviews")]
internal sealed class ReviewsController(IReviewService reviewService) : ApiControllerBase
{
    private readonly IReviewService _reviewService = reviewService;

    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResponse<ReviewDto>), 200)]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = PaginationDefaults.DefaultPageSize)
    {
        if (pageSize > PaginationDefaults.MaxPageSize) pageSize = PaginationDefaults.MaxPageSize;
        if (page < 1) page = 1;

        var result = await _reviewService.GetAllAsync(page, pageSize);
        return Ok(new PaginatedResponse<ReviewDto>
        {
            Items = result.Items,
            Page = page,
            PageSize = pageSize,
            TotalCount = result.TotalCount
        });
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ReviewDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var review = await _reviewService.GetByIdAsync(id);
        if (review is null)
            return Problem(statusCode: 404, title: "Not Found", detail: $"Review {id} not found.");

        return Ok(review);
    }

    [Authorize]
    [HttpPost]
    [ProducesResponseType(typeof(ReviewDto), 201)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    [ProducesResponseType(422)]
    public async Task<IActionResult> Create([FromBody] CreateReviewDto req)
    {
        if (!TryGetProfileId(out var profileId))
            return Problem(statusCode: 401, title: "Unauthorized", detail: "Invalid user identity.");

        try
        {
            var review = await _reviewService.CreateAsync(profileId, req);
            return CreatedAtAction(nameof(GetById), new { id = review.Id }, review);
        }
        catch (BusinessRuleException ex)
        {
            return Problem(statusCode: 422, title: "Unprocessable Entity", detail: ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            return Problem(statusCode: 404, title: "Not Found", detail: ex.Message);
        }
    }

    [Authorize]
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ReviewDto), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    [ProducesResponseType(422)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateReviewDto req)
    {
        if (!TryGetProfileId(out var profileId))
            return Problem(statusCode: 401, title: "Unauthorized", detail: "Invalid user identity.");

        try
        {
            var review = await _reviewService.UpdateAsync(profileId, req.WithRouteId(id));
            return Ok(review);
        }
        catch (BusinessRuleException ex)
        {
            return Problem(statusCode: 403, title: "Forbidden", detail: ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            return Problem(statusCode: 404, title: "Not Found", detail: ex.Message);
        }
    }

    [Authorize]
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Delete(Guid id)
    {
        if (!TryGetProfileId(out var profileId))
            return Problem(statusCode: 401, title: "Unauthorized", detail: "Invalid user identity.");

        try
        {
            await _reviewService.DeleteAsync(profileId, id);
            return NoContent();
        }
        catch (BusinessRuleException ex)
        {
            return Problem(statusCode: 403, title: "Forbidden", detail: ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            return Problem(statusCode: 404, title: "Not Found", detail: ex.Message);
        }
    }

    [HttpGet("booking/{bookingId:guid}")]
    [ProducesResponseType(typeof(ReviewDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetByBooking(Guid bookingId)
    {
        var review = await _reviewService.GetByBookingAsync(bookingId);
        if (review is null)
            return Problem(statusCode: 404, title: "Not Found", detail: $"No review found for booking {bookingId}.");

        return Ok(review);
    }

    [HttpGet("profile/{profileId:guid}")]
    [ProducesResponseType(typeof(PaginatedResponse<ReviewDto>), 200)]
    public async Task<IActionResult> GetByProfile(Guid profileId, [FromQuery] int page = 1, [FromQuery] int pageSize = PaginationDefaults.DefaultPageSize)
    {
        if (pageSize > PaginationDefaults.MaxPageSize) pageSize = PaginationDefaults.MaxPageSize;
        if (page < 1) page = 1;

        var result = await _reviewService.GetByProfileAsync(profileId, page, pageSize);
        return Ok(new PaginatedResponse<ReviewDto>
        {
            Items = result.Items,
            Page = page,
            PageSize = pageSize,
            TotalCount = result.TotalCount
        });
    }
}
