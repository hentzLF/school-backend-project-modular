using AgriMarket.Modules.Marketplace.Services;
using AgriMarket.Shared.Exceptions;
using AgriMarket.Modules.Bookings.Services;
using AgriMarket.Web.Areas.Admin.ViewModels;
using AgriMarket.Web.Mappers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace AgriMarket.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = "AdminOnly")]
public class ListingsController(IListingService listingService, ICategoryService categoryService, IBookingService bookingService) : Controller
{
    public async Task<IActionResult> Index(bool? active)
    {
        var listings = active.HasValue && active.Value
            ? await listingService.GetActiveListingsAsync()
            : await listingService.GetAllAsync();

        if (active.HasValue && !active.Value)
            listings = listings.Where(l => !l.IsActive).ToList();

        listings = listings.OrderBy(l => l.Title).ToList();

        var vm = new ListingListViewModel
        {
            TotalCount = listings.Count(),
            FilterActive = active,
            Listings = listings.Select(l => l.ToAdminListItem())
        };

        return View(vm);
    }

    public async Task<IActionResult> Details(Guid id)
    {
        var listing = await listingService.GetByIdAsync(id);
        if (listing == null) return NotFound();

        var bookingsCount = await bookingService.GetCountByListingAsync(id);
        return View(listing.ToAdminDetailVm(bookingsCount));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(Guid id)
    {
        var listing = await listingService.GetByIdAsync(id);
        if (listing == null) return NotFound();

        var vm = listing.ToAdminEditVm();
        vm.Categories = await GetCategorySelectList(listing.ServiceCategoryId);
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(ListingEditViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            vm.Categories = await GetCategorySelectList(vm.ServiceCategoryId);
            return View(vm);
        }

        try
        {
            await listingService.AdminUpdateAsync(vm.ToUpdateListingDto());
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (BusinessRuleException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToAction(nameof(Edit), new { id = vm.Id });
        }

        return RedirectToAction(nameof(Details), new { id = vm.Id });
    }

    [HttpGet]
    public async Task<IActionResult> Delete(Guid id)
    {
        var listing = await listingService.GetByIdAsync(id);
        if (listing == null) return NotFound();

        return View(listing.ToAdminListItem());
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        try
        {
            await listingService.AdminDeleteAsync(id);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (BusinessRuleException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToAction(nameof(Index));
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleActive(Guid id)
    {
        try
        {
            await listingService.AdminToggleActiveAsync(id);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    private async Task<IEnumerable<SelectListItem>> GetCategorySelectList(Guid selectedId)
    {
        var categories = await categoryService.GetAllAsync();
        return categories.OrderBy(c => c.Name).Select(c => new SelectListItem
        {
            Value = c.Id.ToString(),
            Text = c.Name,
            Selected = c.Id == selectedId
        });
    }
}
