using AgriMarket.Modules.Messaging.Contracts;
using AgriMarket.Modules.Users.Enums;
using AgriMarket.Modules.Users.Security;
using AgriMarket.Modules.Users.Services;
using AgriMarket.Modules.Bookings.Enums;
using AgriMarket.Web.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AgriMarket.Web.Areas.Admin.Controllers;

[Area("Admin")]
internal class AccountController(IUserService userService, IPasswordHasher passwordHasher) : Controller
{

    [HttpGet]
    public IActionResult Login() => View(new LoginViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var user = await userService.GetByEmailAsync(model.Email);

        if (user == null || !passwordHasher.Verify(model.Password, user.PasswordHash))
        {
            ModelState.AddModelError(string.Empty, "Invalid email or password");
            return View(model);
        }

        if (user.LockoutEnd.HasValue && user.LockoutEnd > DateTime.UtcNow)
        {
            ModelState.AddModelError(string.Empty, "Your account is locked");
            return View(model);
        }

        var hasAdminRole = user.Roles?.Any(r => r.Role == RoleType.Admin) ?? false;
        if (!hasAdminRole)
        {
            ModelState.AddModelError(string.Empty, "You do not have administrator access");
            return View(model);
        }

        var profile = user.Profile;
        if (profile == null)
        {
            ModelState.AddModelError(string.Empty, "User profile not found");
            return View(model);
        }

        await SignInAsync(user, profile);
        return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
    }

    [HttpGet]
    [Authorize(Policy = "AdminOnly")]
    public IActionResult Register() => View(new RegisterViewModel());

    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var existingUser = await userService.GetByEmailAsync(model.Email);
        if (existingUser != null)
        {
            ModelState.AddModelError(string.Empty, "An account with this email already exists");
            return View(model);
        }

        var user = new AgriMarket.Modules.Users.Entities.AppUser
        {
            Id = Guid.NewGuid(),
            Email = model.Email,
            PasswordHash = passwordHasher.Hash(model.Password),
            CreatedAt = DateTime.UtcNow
        };

        var profile = new AgriMarket.Modules.Users.Entities.UserProfile
        {
            Id = Guid.NewGuid(),
            FirstName = model.FirstName,
            LastName = model.LastName,
            AppUserId = user.Id
        };

        await userService.CreateUserWithProfileAsync(user, profile, RoleType.Admin);

        await SignInAsync(user, profile);
        return RedirectToAction("Index", "Dashboard", new { area = "Admin" });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Login", "Account", new { area = "Admin" });
    }

    public IActionResult AccessDenied() => View();

    private async Task SignInAsync(AgriMarket.Modules.Users.Entities.AppUser user, AgriMarket.Modules.Users.Entities.UserProfile profile)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Name, $"{profile.FirstName} {profile.LastName}"),
            new("profileId", profile.Id.ToString()),
            new(ClaimTypes.Role, "Admin")
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
    }
}
