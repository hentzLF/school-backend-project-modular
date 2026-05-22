using AgriMarket.Modules.Users.Dtos;
using AgriMarket.Modules.Users.Mappers;
using AgriMarket.Modules.Users.Services;
using AgriMarket.Shared.Dtos;
using AgriMarket.Shared.Web;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgriMarket.Modules.Users.Controllers;

internal sealed record UpdateProfileRequest(string FirstName, string LastName, string? Bio, string? AvatarUrl);

[ApiController]
[ApiVersion("1")]
[Route("api/v{version:apiVersion}/users")]
internal sealed class UsersController(IUserService userService) : ApiControllerBase
{
    private readonly IUserService _userService = userService;

    [Authorize]
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResponse<UserProfileDto>), 200)]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = PaginationDefaults.DefaultPageSize)
    {
        if (pageSize > PaginationDefaults.MaxPageSize) pageSize = PaginationDefaults.MaxPageSize;
        if (page < 1) page = 1;

        var result = await _userService.GetAllProfilesAsync(page, pageSize);
        var items = result.Items.Select(UserApiMapper.HideEmail);
        return Ok(new PaginatedResponse<UserProfileDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = result.TotalCount
        });
    }

    [Authorize]
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(UserProfileDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var callerUserId = GetCallerUserId();
        var isAdmin = User.Claims.Any(c =>
            c.Type == "role" && string.Equals(c.Value, "Admin", StringComparison.OrdinalIgnoreCase));

        var profile = await _userService.GetProfileByIdAsync(id, callerUserId, isAdmin);
        if (profile is null)
            return Problem(statusCode: 404, title: "Not Found", detail: $"UserProfile {id} not found.");

        return Ok(profile);
    }

    [Authorize]
    [HttpPut("profile")]
    [ProducesResponseType(typeof(UserProfileDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest req)
    {
        if (!TryGetUserId(out var userId))
            return Problem(statusCode: 401, title: "Unauthorized", detail: "Invalid user identity.");

        var existing = await _userService.GetProfileByUserIdAsync(userId);
        if (existing is null)
            return Problem(statusCode: 404, title: "Not Found", detail: "User profile not found.");

        var updated = new UserProfileDto
        {
            Id = existing.Id,
            FirstName = req.FirstName,
            LastName = req.LastName,
            Bio = req.Bio,
            AvatarUrl = req.AvatarUrl,
            AppUserId = existing.AppUserId,
            Email = existing.Email,
            CreatedAt = existing.CreatedAt,
            Roles = existing.Roles
        };

        await _userService.UpdateProfileAsync(updated);
        return Ok(updated);
    }
}
