using AgriMarket.Modules.Bookings.Services;
using AgriMarket.Modules.Bookings.Enums;
using AgriMarket.Web.Areas.Admin.ViewModels;
using AgriMarket.Web.Mappers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AgriMarket.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = "AdminOnly")]
public class BookingsController : Controller
{
    private readonly IBookingService _bookingService;

    public BookingsController(IBookingService bookingService)
    {
        _bookingService = bookingService;
    }

    public async Task<IActionResult> Index(BookingStatus? status)
    {
        var bookings = await _bookingService.GetAllAsync(status);

        var vm = new BookingListViewModel
        {
            TotalCount = bookings.Count(),
            FilterStatus = status,
            Bookings = bookings.Select(b => b.ToAdminListItem())
        };

        return View(vm);
    }

    public async Task<IActionResult> Details(Guid id)
    {
        var booking = await _bookingService.GetByIdAsync(id);
        if (booking == null) return NotFound();

        return View(booking.ToAdminDetailVm());
    }

    [HttpGet]
    public async Task<IActionResult> Edit(Guid id)
    {
        var booking = await _bookingService.GetByIdAsync(id);
        if (booking == null) return NotFound();

        var vm = new BookingEditViewModel
        {
            Id = booking.Id,
            Status = booking.Status,
            ListingTitle = booking.ListingTitle,
            ClientName = booking.ClientName,
            Statuses = GetStatusSelectList(booking.Status)
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(BookingEditViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            vm.Statuses = GetStatusSelectList(vm.Status);
            return View(vm);
        }

        await _bookingService.UpdateStatusAsync(vm.Id, vm.Status);
        return RedirectToAction(nameof(Details), new { id = vm.Id });
    }

    [HttpGet]
    public async Task<IActionResult> Delete(Guid id)
    {
        var booking = await _bookingService.GetByIdAsync(id);
        if (booking == null) return NotFound();

        return View(booking.ToAdminListItem());
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        await _bookingService.DeleteAsync(id);
        return RedirectToAction(nameof(Index));
    }

    private static IEnumerable<SelectListItem> GetStatusSelectList(BookingStatus selected)
    {
        return Enum.GetValues<BookingStatus>().Select(s => new SelectListItem
        {
            Value = s.ToString(),
            Text = s.ToString(),
            Selected = s == selected
        });
    }
}
