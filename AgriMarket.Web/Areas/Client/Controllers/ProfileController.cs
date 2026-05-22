using AgriMarket.Modules.Users.Services;
using AgriMarket.Web.Areas.Client.ViewModels.Profile;
using AgriMarket.Web.Mappers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AgriMarket.Web.Areas.Client.Controllers;

[Area("Client")]
[Authorize(Policy = "ClientOnly")]
internal class ProfileController(IUserService userService) : Controller
{
    public async Task<IActionResult> Index()
    {
        var profile = await GetProfileAsync();
        if (profile == null) return Unauthorized();

        var role = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;
        return View(profile.ToProfileViewModel(role));
    }

    [HttpGet]
    public async Task<IActionResult> Edit()
    {
        var profile = await GetProfileAsync();
        if (profile == null) return Unauthorized();

        return View(profile.ToEditProfileViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditProfileViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var profileDto = await GetProfileAsync();
        if (profileDto == null) return Unauthorized();

        await userService.UpdateProfileAsync(new AgriMarket.Modules.Users.Dtos.UserProfileDto
        {
            Id = profileDto.Id,
            FirstName = model.FirstName,
            LastName = model.LastName,
            Bio = model.Bio,
            AvatarUrl = model.AvatarUrl,
            AppUserId = profileDto.AppUserId,
            Email = profileDto.Email
        });

        TempData["SuccessMessage"] = "Profile updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    private async Task<AgriMarket.Modules.Users.Dtos.UserProfileDto?> GetProfileAsync()
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdStr, out var userId)) return null;
        return await userService.GetProfileByUserIdAsync(userId);
    }
}
