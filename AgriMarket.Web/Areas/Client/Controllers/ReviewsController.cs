using AgriMarket.Shared.Exceptions;
using AgriMarket.Modules.Users.Services;
using AgriMarket.Modules.Bookings.Services;
using AgriMarket.Web.Areas.Client.ViewModels.Reviews;
using AgriMarket.Web.Mappers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AgriMarket.Web.Areas.Client.Controllers;

[Area("Client")]
[Authorize(Policy = "ClientOnly")]
internal class ReviewsController(
    IReviewService reviewService,
    IBookingService bookingService,
    IUserService userService) : Controller
{
    private const int PageSize = 10;

    public async Task<IActionResult> Create(Guid bookingId)
    {
        var booking = await bookingService.GetByIdAsync(bookingId);
        if (booking == null) return NotFound();

        var vm = new CreateReviewViewModel
        {
            BookingId = bookingId,
            BookingTitle = booking.ListingTitle
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateReviewViewModel model)
    {
        if (!ModelState.IsValid)
            return await ReloadCreateView(model);

        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        try
        {
            var dto = model.ToCreateDto();
            await reviewService.CreateAsync(userId.Value, dto);
            return RedirectToAction("Details", "Bookings", new { area = "Client", id = model.BookingId });
        }
        catch (BusinessRuleException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToAction("Details", "Bookings", new { area = "Client", id = model.BookingId });
        }
    }

    public async Task<IActionResult> Edit(Guid id)
    {
        var review = await reviewService.GetByIdAsync(id);
        if (review == null) return NotFound();

        var booking = await bookingService.GetByIdAsync(review.BookingId);
        var bookingTitle = booking?.ListingTitle ?? "Unknown";

        var vm = review.ToEditViewModel(bookingTitle);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditReviewViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        try
        {
            var dto = model.ToUpdateDto();
            await reviewService.UpdateAsync(userId.Value, dto);
            return RedirectToAction("Details", "Bookings", new { area = "Client", id = model.BookingId });
        }
        catch (BusinessRuleException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToAction("Details", "Bookings", new { area = "Client", id = model.BookingId });
        }
    }

    public async Task<IActionResult> Delete(Guid id)
    {
        var review = await reviewService.GetByIdAsync(id);
        if (review == null) return NotFound();

        var vm = review.ToDeleteViewModel();
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [ActionName("Delete")]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var review = await reviewService.GetByIdAsync(id);
        if (review == null) return NotFound();

        try
        {
            await reviewService.DeleteAsync(userId.Value, id);
            return RedirectToAction("Details", "Bookings", new { area = "Client", id = review.BookingId });
        }
        catch (BusinessRuleException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToAction("Details", "Bookings", new { area = "Client", id = review.BookingId });
        }
    }

    public async Task<IActionResult> ForProvider(Guid profileId, int page = 1)
    {
        var profile = await userService.GetProfileByIdAsync(profileId);
        if (profile == null) return NotFound();

        var (reviews, totalCount) = await reviewService.GetByProfileAsync(profileId, page, PageSize);
        var stats = await reviewService.GetRatingStatsForProfileAsync(profileId);

        var totalPages = (int)Math.Ceiling((double)totalCount / PageSize);
        var providerName = $"{profile.FirstName} {profile.LastName}";

        var vm = ReviewViewModelMapper.ToReviewListViewModel(
            reviews, stats, profileId, providerName, page, totalPages);

        return View(vm);
    }

    private async Task<IActionResult> ReloadCreateView(CreateReviewViewModel model)
    {
        var booking = await bookingService.GetByIdAsync(model.BookingId);
        model.BookingTitle = booking?.ListingTitle ?? "Unknown";
        return View("Create", model);
    }

    private Guid? GetUserId()
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (Guid.TryParse(userIdStr, out var userId)) return userId;
        return null;
    }
}
