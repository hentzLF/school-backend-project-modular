using AgriMarket.Modules.Users.Dtos;
using AgriMarket.Modules.Bookings.Services;
using AgriMarket.Modules.Marketplace.Services;
using AgriMarket.Modules.Users.Services;
using AgriMarket.Web.Areas.Admin.ViewModels;
using AgriMarket.Web.Mappers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AgriMarket.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = "AdminOnly")]
internal class UsersController : Controller
{
    private readonly IUserService _userService;
    private readonly IBookingService _bookingService;
    private readonly IListingService _listingService;

    public UsersController(IUserService userService, IBookingService bookingService, IListingService listingService)
    {
        _userService = userService;
        _bookingService = bookingService;
        _listingService = listingService;
    }

    public async Task<IActionResult> Index()
    {
        var users = await _userService.GetAllUsersAsync();

        var vm = new UserListViewModel
        {
            TotalCount = users.Count(),
            Users = users.Select(u => u.ToUserListItem())
        };

        return View(vm);
    }

    public async Task<IActionResult> Details(Guid id)
    {
        var profile = await _userService.GetUserByIdAsync(id, GetCallerUserId(), isAdmin: true);
        if (profile == null) return NotFound();

        var listings = await _listingService.GetByProviderAsync(profile.Id);
        var (_, bookingsCount) = await _bookingService.GetAllForProfileAsync(profile.Id, 1, 1);

        var vm = new UserDetailViewModel
        {
            Id = profile.Id,
            Email = profile.Email ?? string.Empty,
            CreatedAt = profile.CreatedAt,
            IsLocked = profile.IsLocked,
            LockoutEnd = profile.LockoutEnd,
            ListingsCount = listings.Count(),
            BookingsCount = bookingsCount,
            Profiles = [new UserProfileDetailViewModel
            {
                Id = profile.Id,
                FirstName = profile.FirstName,
                LastName = profile.LastName,
                Bio = profile.Bio,
                AvatarUrl = profile.AvatarUrl,
                Roles = profile.Roles
            }]
        };

        return View(vm);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(Guid id)
    {
        var profile = await _userService.GetUserByIdAsync(id, GetCallerUserId(), isAdmin: true);
        if (profile == null) return NotFound();

        var lockoutEnd = await GetLockoutEnd(profile);
        var vm = new UserEditViewModel
        {
            Id = profile.Id,
            Email = profile.Email ?? string.Empty,
            LockoutEnd = lockoutEnd
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(UserEditViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);

        var profile = await _userService.GetUserByIdAsync(vm.Id, GetCallerUserId(), isAdmin: true);
        if (profile == null) return NotFound();

        await _userService.UpdateUserAsync(profile.AppUserId, vm.Email, vm.LockoutEnd);

        return RedirectToAction(nameof(Details), new { id = vm.Id });
    }

    [HttpGet]
    public async Task<IActionResult> Delete(Guid id)
    {
        var profile = await _userService.GetUserByIdAsync(id, GetCallerUserId(), isAdmin: true);
        if (profile == null) return NotFound();

        return View(profile.ToUserListItem());
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        var profile = await _userService.GetUserByIdAsync(id, GetCallerUserId(), isAdmin: true);
        if (profile == null) return NotFound();

        await _userService.DeleteUserAsync(profile.AppUserId);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Lock(Guid id)
    {
        var profile = await _userService.GetUserByIdAsync(id, GetCallerUserId(), isAdmin: true);
        if (profile == null) return NotFound();

        await _userService.LockUserAsync(profile.AppUserId);
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Unlock(Guid id)
    {
        var profile = await _userService.GetUserByIdAsync(id, GetCallerUserId(), isAdmin: true);
        if (profile == null) return NotFound();

        await _userService.UnlockUserAsync(profile.AppUserId);
        return RedirectToAction(nameof(Details), new { id });
    }

    private Guid? GetCallerUserId()
    {
        return Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId) ? userId : null;
    }

    private async Task<DateTime?> GetLockoutEnd(UserProfileDto profile)
    {
        if (string.IsNullOrWhiteSpace(profile.Email))
            return null;

        var user = await _userService.GetByEmailAsync(profile.Email);
        return user?.LockoutEnd;
    }
}
