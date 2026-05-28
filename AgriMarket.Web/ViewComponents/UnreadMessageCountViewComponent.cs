using AgriMarket.Modules.Messaging.Services;
using AgriMarket.Modules.Users.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AgriMarket.Web.ViewComponents;

internal sealed class UnreadMessageCountViewComponent(
    IMessagingService messagingService,
    IUserService userService) : ViewComponent
{
    public async Task<IViewComponentResult> InvokeAsync()
    {
        var userId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userId, out var parsedUserId))
            return View(0);

        var profile = await userService.GetProfileByUserIdAsync(parsedUserId);
        if (profile == null)
            return View(0);

        var unreadCount = await messagingService.GetUnreadCountAsync(profile.Id);
        return View(unreadCount.UnreadCount);
    }
}
