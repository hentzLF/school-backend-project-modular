using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;

namespace AgriMarket.Web.Controllers;

internal class CultureController : Controller
{
    [HttpPost]
    public IActionResult SetCulture(string culture, string returnUrl)
    {
        Response.Cookies.Append(
            CookieRequestCultureProvider.DefaultCookieName,
            CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
            new CookieOptions { Expires = DateTimeOffset.UtcNow.AddDays(365) }
        );

        return LocalRedirect(returnUrl ?? "/");
    }
}
