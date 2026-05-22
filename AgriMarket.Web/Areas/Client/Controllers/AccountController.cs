using AgriMarket.Modules.Messaging.Contracts;
using AgriMarket.Modules.Users.Security;
using AgriMarket.Modules.Users.Services;
using AgriMarket.Modules.Users.Enums;
using AgriMarket.Web.Areas.Client.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AgriMarket.Web.Areas.Client.Controllers;

[Area("Client")]
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

        var hasClientRole = user.Roles?.Any(r => r.Role == RoleType.Client) ?? false;
        if (!hasClientRole)
        {
            ModelState.AddModelError(string.Empty, "You do not have client access");
            return View(model);
        }

        var profile = user.Profile;
        if (profile == null)
        {
            ModelState.AddModelError(string.Empty, "User profile not found");
            return View(model);
        }

        var roles = user.Roles!.Select(r => r.Role).ToList();
        await SignInAsync(user, profile, roles);
        return RedirectToAction("Index", "Listings", new { area = "Client" });
    }

    [HttpGet]
    public IActionResult Register() => View(new RegisterViewModel());

    [HttpPost]
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

        await userService.CreateUserWithProfileAsync(user, profile, RoleType.Client);

        await SignInAsync(user, profile, [RoleType.Client]);
        return RedirectToAction("Index", "Listings", new { area = "Client" });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Login");
    }

    public IActionResult AccessDenied() => View();

    private async Task SignInAsync(
        AgriMarket.Modules.Users.Entities.AppUser user,
        AgriMarket.Modules.Users.Entities.UserProfile profile,
        IEnumerable<RoleType> roles)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Name, $"{profile.FirstName} {profile.LastName}"),
            new("profileId", profile.Id.ToString()),
        };

        foreach (var role in roles)
            claims.Add(new Claim(ClaimTypes.Role, role.ToString()));

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
    }
}
