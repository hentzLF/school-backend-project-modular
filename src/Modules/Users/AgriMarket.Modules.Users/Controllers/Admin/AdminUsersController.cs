using AgriMarket.Modules.Users.Dtos;
using AgriMarket.Modules.Users.Services;
using AgriMarket.Shared.Dtos;
using AgriMarket.Shared.Web;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgriMarket.Modules.Users.Controllers.Admin;

[ApiController]
[ApiVersion("1")]
[Route("api/v{version:apiVersion}/admin/users")]
[Authorize(Policy = "AdminOnly")]
internal sealed class AdminUsersController(IUserService userService) : ApiControllerBase
{
    private readonly IUserService _userService = userService;

    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResponse<UserProfileDto>), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        if (pageSize > 100) pageSize = 100;
        if (page < 1) page = 1;

        var result = await _userService.GetAllProfilesAsync(page, pageSize);
        return Ok(new PaginatedResponse<UserProfileDto>
        {
            Items = result.Items,
            Page = page,
            PageSize = pageSize,
            TotalCount = result.TotalCount
        });
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(UserProfileDto), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var profile = await _userService.GetProfileByIdAsync(id, callerUserId: null, isAdmin: true);
        if (profile is null)
            return Problem(statusCode: 404, title: "Not Found", detail: $"UserProfile {id} not found.");

        return Ok(profile);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(UserProfileDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UserProfileDto profile)
    {
        var existing = await _userService.GetProfileByIdAsync(id, callerUserId: null, isAdmin: true);
        if (existing is null)
            return Problem(statusCode: 404, title: "Not Found", detail: $"UserProfile {id} not found.");

        var updated = new UserProfileDto
        {
            Id = id,
            FirstName = profile.FirstName,
            LastName = profile.LastName,
            Bio = profile.Bio,
            AvatarUrl = profile.AvatarUrl,
            AppUserId = existing.AppUserId,
            Email = existing.Email,
            CreatedAt = existing.CreatedAt,
            Roles = existing.Roles
        };

        await _userService.UpdateProfileAsync(updated);
        return Ok(updated);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var existing = await _userService.GetProfileByIdAsync(id, callerUserId: null, isAdmin: true);
        if (existing is null)
            return Problem(statusCode: 404, title: "Not Found", detail: $"UserProfile {id} not found.");

        await _userService.DeleteUserAsync(existing.AppUserId);
        return NoContent();
    }
}
