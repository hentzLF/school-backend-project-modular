using AgriMarket.Shared.Exceptions;
using AgriMarket.Modules.Bookings.Services;
using AgriMarket.Modules.Users.Services;
using AgriMarket.Modules.Marketplace.Services;
using AgriMarket.Web.Areas.Client.ViewModels.Bookings;
using AgriMarket.Web.Areas.Client.ViewModels.Listings;
using AgriMarket.Web.Areas.Client.ViewModels.Reviews;
using AgriMarket.Web.Mappers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AgriMarket.Web.Areas.Client.Controllers;

[Area("Client")]
public class ListingsController(
    IListingService listingService,
    IBookingService bookingService,
    IReviewService reviewService,
    IEquipmentService equipmentService) : Controller
{
    public async Task<IActionResult> Index()
    {
        var listings = await listingService.GetAllAsync();

        var vm = new ListingIndexViewModel
        {
            Listings = listings.Select(l =>
            {
                var item = l.ToClientIndexItem();
                item.RatingStats = new RatingStatsViewModel
                {
                    AverageRating = l.AverageRating,
                    ReviewCount = l.ReviewCount
                };
                return item;
            })
        };

        return View(vm);
    }

    public async Task<IActionResult> Details(Guid id)
    {
        var listing = await listingService.GetByIdAsync(id);
        if (listing == null)
            return NotFound();

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var isOwnListing = userId != null &&
                           listing.ProviderUserId.HasValue &&
                           listing.ProviderUserId.Value.ToString() == userId;

        var vm = listing.ToClientDetails(isOwnListing);

        var stats = await reviewService.GetRatingStatsForListingAsync(id);
        vm.RatingStats = stats.ToRatingStatsViewModel();
        vm.ProviderProfileId = listing.UserProfileId;

        var assignedEquipment = await equipmentService.GetByListingAsync(id);
        vm.Equipment = assignedEquipment.Select(e => e.ToListItem()).ToList();

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Policy = "ClientOnly")]
    public async Task<IActionResult> Book(CreateBookingViewModel model)
    {
        if (!ModelState.IsValid)
            return RedirectToAction(nameof(Details), new { id = model.ServiceListingId });

        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdStr, out var userId))
            return Unauthorized();

        try
        {
            var booking = await bookingService.CreateAsync(userId, model.ToCreateBookingDto());
            return RedirectToAction("Details", "Bookings", new { area = "Client", id = booking.Id });
        }
        catch (BusinessRuleException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToAction(nameof(Details), new { id = model.ServiceListingId });
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }
}
