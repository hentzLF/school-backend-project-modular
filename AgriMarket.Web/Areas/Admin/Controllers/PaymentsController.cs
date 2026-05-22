using AgriMarket.Modules.Bookings.Services;
using AgriMarket.Modules.Bookings.Enums;
using AgriMarket.Modules.Users.Services;
using AgriMarket.Web.Areas.Admin.ViewModels;
using AgriMarket.Web.Mappers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgriMarket.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = "AdminOnly")]
public class PaymentsController : Controller
{
    private readonly IPaymentService _paymentService;
    private readonly IBookingService _bookingService;
    private readonly IUserService _userService;

    public PaymentsController(
        IPaymentService paymentService,
        IBookingService bookingService,
        IUserService userService)
    {
        _paymentService = paymentService;
        _bookingService = bookingService;
        _userService = userService;
    }

    public async Task<IActionResult> Index(PaymentStatus? status)
    {
        var payments = await _paymentService.GetAllAsync(status);

        var vm = new PaymentListViewModel
        {
            TotalCount = payments.Count(),
            FilterStatus = status,
            Payments = payments.Select(p => p.ToAdminListItem())
        };

        return View(vm);
    }

    public async Task<IActionResult> Details(Guid id)
    {
        var payment = await _paymentService.GetByIdAsync(id);

        if (payment == null) return NotFound();

        var booking = await _bookingService.GetByIdAsync(payment.BookingId);
        string providerName = "Unknown";
        if (booking != null)
        {
            var provider = await _userService.GetProfileByIdAsync(booking.ProviderProfileId);
            if (provider != null)
                providerName = $"{provider.FirstName} {provider.LastName}";
        }

        var vm = new PaymentDetailViewModel
        {
            Id = payment.Id,
            Amount = payment.Amount,
            PlatformFee = payment.PlatformFee,
            Status = payment.Status,
            CreatedAt = payment.CreatedAt,
            ReleasedAt = payment.ReleasedAt,
            BookingId = payment.BookingId,
            BookingStatus = booking?.Status ?? default,
            ListingId = booking?.ServiceListingId ?? default,
            ListingTitle = booking?.ListingTitle ?? "Unknown",
            ClientName = booking?.ClientName ?? "Unknown",
            ClientProfileId = booking?.ClientProfileId ?? default,
            ProviderName = providerName,
            ProviderProfileId = booking?.ProviderProfileId ?? default
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Resolve(DisputeResolveViewModel vm)
    {
        if (!ModelState.IsValid)
            return RedirectToAction(nameof(Details), new { id = vm.PaymentId });

        var payment = await _paymentService.GetByIdAsync(vm.PaymentId);
        if (payment == null) return NotFound();

        if (payment.Status != PaymentStatus.Disputed)
        {
            TempData["Error"] = "Only disputed payments can be resolved";
            return RedirectToAction(nameof(Details), new { id = vm.PaymentId });
        }

        if (!Enum.IsDefined(vm.Resolution))
        {
            TempData["Error"] = "Invalid resolution option";
            return RedirectToAction(nameof(Details), new { id = vm.PaymentId });
        }

        await _paymentService.ResolveDisputeAsync(vm.PaymentId, vm.Resolution);

        return RedirectToAction(nameof(Details), new { id = vm.PaymentId });
    }
}
