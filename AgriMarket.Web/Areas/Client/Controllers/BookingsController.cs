using AgriMarket.Shared.Exceptions;
using AgriMarket.Modules.Bookings.Dtos.Payments;
using AgriMarket.Modules.Bookings.Services;
using AgriMarket.Modules.Users.Services;
using AgriMarket.Modules.Bookings.Enums;
using AgriMarket.Web.Areas.Client.ViewModels.Bookings;
using AgriMarket.Web.Areas.Client.ViewModels.Payments;
using AgriMarket.Web.Mappers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AgriMarket.Web.Areas.Client.Controllers;

[Area("Client")]
[Authorize(Policy = "ClientOnly")]
public class BookingsController(
    IBookingService bookingService,
    IUserService userService,
    IClientPaymentService clientPaymentService,
    IReviewService reviewService) : Controller
{
    public async Task<IActionResult> Index()
    {
        var clientProfile = await GetClientProfileAsync();
        if (clientProfile == null) return Unauthorized();

        var bookings = await bookingService.GetByClientAsync(clientProfile.Id);

        var vm = new BookingIndexViewModel
        {
            Bookings = bookings.Select(b => b.ToClientIndexItem())
        };

        return View(vm);
    }

    public async Task<IActionResult> Details(Guid id)
    {
        var clientProfile = await GetClientProfileAsync();
        if (clientProfile == null) return Unauthorized();

        var booking = await bookingService.GetByIdAsync(id);
        if (booking == null) return NotFound();

        if (booking.ClientProfileId != clientProfile.Id && booking.ProviderProfileId != clientProfile.Id)
            return RedirectToAction("AccessDenied", "Account");

        var vm = booking.ToClientDetailsVm();
        await LoadReviewForBooking(vm, id);
        return View(vm);
    }

    private async Task LoadReviewForBooking(BookingDetailsViewModel vm, Guid bookingId)
    {
        var reviewDto = await reviewService.GetByBookingAsync(bookingId);
        if (reviewDto != null)
        {
            vm.Review = reviewDto.ToViewModel();
        }
    }

    public async Task<IActionResult> Checkout(Guid id)
    {
        var clientProfile = await GetClientProfileAsync();
        if (clientProfile == null) return Unauthorized();

        var booking = await bookingService.GetByIdAsync(id);
        if (booking == null) return NotFound();

        if (booking.ClientProfileId != clientProfile.Id)
            return RedirectToAction("AccessDenied", "Account");

        if (booking.Status != BookingStatus.AwaitingPayment)
            return RedirectToAction(nameof(Details), new { id });

        var vm = BuildCheckoutViewModel(booking);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Checkout(CheckoutSubmitViewModel model)
    {
        var clientProfile = await GetClientProfileAsync();
        if (clientProfile == null) return Unauthorized();

        if (!ModelState.IsValid)
            return await ReloadCheckoutView(model.BookingId);

        try
        {
            var request = new PayRequest(model.BookingId, model.Method);
            var receipt = await clientPaymentService.PayAsync(clientProfile.Id, request);
            return RedirectToAction("Receipt", "Payments", new { area = "Client", id = receipt.PaymentId });
        }
        catch (BusinessRuleException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToAction(nameof(Details), new { id = model.BookingId });
        }
    }

    private static CheckoutViewModel BuildCheckoutViewModel(AgriMarket.Modules.Bookings.Dtos.Bookings.BookingDto booking)
    {
        var platformFee = booking.TotalPrice * 0.05m;
        return new CheckoutViewModel
        {
            BookingId = booking.Id,
            ListingTitle = booking.ListingTitle,
            AreaInHectares = booking.AreaInHectares,
            ServiceTotal = booking.TotalPrice,
            PlatformFee = platformFee,
            GrandTotal = booking.TotalPrice + platformFee
        };
    }

    private async Task<IActionResult> ReloadCheckoutView(Guid bookingId)
    {
        var booking = await bookingService.GetByIdAsync(bookingId);
        if (booking == null) return NotFound();

        var vm = BuildCheckoutViewModel(booking);
        return View("Checkout", vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(Guid id)
    {
        var clientProfile = await GetClientProfileAsync();
        if (clientProfile == null) return Unauthorized();

        var booking = await bookingService.GetByIdAsync(id);
        if (booking == null) return NotFound();

        if (booking.ClientProfileId != clientProfile.Id)
            return RedirectToAction("AccessDenied", "Account");

        await bookingService.UpdateStatusAsync(id, BookingStatus.Cancelled, clientProfile.Id);
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmCompletion(Guid id)
    {
        var clientProfile = await GetClientProfileAsync();
        if (clientProfile == null) return Unauthorized();

        var booking = await bookingService.GetByIdAsync(id);
        if (booking == null) return NotFound();

        if (booking.ClientProfileId != clientProfile.Id)
            return RedirectToAction("AccessDenied", "Account");

        if (booking.Status != BookingStatus.ProviderCompleted)
            return RedirectToAction(nameof(Details), new { id });

        await bookingService.UpdateStatusAsync(id, BookingStatus.ClientConfirmed, clientProfile.Id);
        return RedirectToAction(nameof(Details), new { id });
    }

    private async Task<AgriMarket.Modules.Users.Dtos.UserProfileDto?> GetClientProfileAsync()
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdStr, out var userId)) return null;
        return await userService.GetProfileByUserIdAsync(userId);
    }
}
