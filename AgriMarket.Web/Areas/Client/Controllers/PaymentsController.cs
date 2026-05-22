using AgriMarket.Modules.Bookings.Services;
using AgriMarket.Modules.Users.Services;
using AgriMarket.Web.Areas.Client.ViewModels.Payments;
using AgriMarket.Web.Mappers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AgriMarket.Web.Areas.Client.Controllers;

[Area("Client")]
[Authorize(Policy = "ClientOnly")]
public class PaymentsController(
    IPaymentService paymentService,
    IClientPaymentService clientPaymentService,
    IUserService userService) : Controller
{
    public async Task<IActionResult> Receipt(Guid id)
    {
        var clientProfile = await GetClientProfileAsync();
        if (clientProfile == null) return Unauthorized();

        var payment = await paymentService.GetByIdAsync(id);
        if (payment == null) return NotFound();

        if (!await IsPaymentOwnedByClient(payment.BookingId, clientProfile.Id))
            return RedirectToAction("AccessDenied", "Account");

        var vm = MapToReceiptViewModel(payment);
        return View(vm);
    }

    public async Task<IActionResult> Index()
    {
        var clientProfile = await GetClientProfileAsync();
        if (clientProfile == null) return Unauthorized();

        var history = await clientPaymentService.GetHistoryAsync(clientProfile.Id);

        var vm = new PaymentHistoryViewModel
        {
            Payments = history.Select(h => h.ToHistoryItemViewModel())
        };

        return View(vm);
    }

    private static ReceiptViewModel MapToReceiptViewModel(AgriMarket.Modules.Bookings.Entities.Payment payment)
    {
        return new ReceiptViewModel
        {
            PaymentId = payment.Id,
            BookingId = payment.BookingId,
            Amount = payment.Amount,
            PlatformFee = payment.PlatformFee,
            TotalCharged = payment.Amount + payment.PlatformFee,
            Method = payment.Method.ToString(),
            Status = payment.Status.ToString(),
            PaidAt = payment.CreatedAt
        };
    }

    private async Task<bool> IsPaymentOwnedByClient(Guid bookingId, Guid clientProfileId)
    {
        var history = await clientPaymentService.GetHistoryAsync(clientProfileId);
        return history.Any(h => h.BookingId == bookingId);
    }

    private async Task<AgriMarket.Modules.Users.Dtos.UserProfileDto?> GetClientProfileAsync()
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdStr, out var userId)) return null;
        return await userService.GetProfileByUserIdAsync(userId);
    }
}
