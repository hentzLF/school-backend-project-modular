using AgriMarket.Shared.Exceptions;
using AgriMarket.Modules.Users.Services;
using AgriMarket.Modules.Marketplace.Dtos.Listings;
using AgriMarket.Modules.Marketplace.Services;
using AgriMarket.Modules.Bookings.Services;
using AgriMarket.Modules.Bookings.Enums;
using AgriMarket.Web.Areas.Client.ViewModels.MyListings;
using AgriMarket.Web.Mappers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;

namespace AgriMarket.Web.Areas.Client.Controllers;

[Area("Client")]
[Authorize(Policy = "ClientOnly")]
internal class MyListingsController(IListingService listingService, ICategoryService categoryService, IBookingService bookingService, IUserService userService, IEquipmentService equipmentService) : Controller
{
    private bool TryGetUserId(out Guid userId)
    {
        return Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out userId);
    }

    private async Task<AgriMarket.Modules.Users.Dtos.UserProfileDto?> GetProviderProfileAsync()
    {
        if (!TryGetUserId(out var userId)) return null;
        return await userService.GetProfileByUserIdAsync(userId);
    }

    public async Task<IActionResult> Index()
    {
        var profile = await GetProviderProfileAsync();
        if (profile == null) return NotFound();

        var listings = await listingService.GetByProviderAsync(profile.Id);
        var viewModel = new MyListingIndexViewModel
        {
            Listings = listings.Select(l => l.ToMyListingIndexItem()).ToList()
        };

        return View(viewModel);
    }

    public async Task<IActionResult> Details(Guid id)
    {
        var profile = await GetProviderProfileAsync();
        if (profile == null) return NotFound();

        var listing = await listingService.GetByIdAsync(id);
        if (listing == null || listing.UserProfileId != profile.Id) return NotFound();

        var bookingCount = await bookingService.GetCountByListingAsync(id);
        var viewModel = listing.ToMyListingDetails(bookingCount);

        var assignedEquipment = await equipmentService.GetByListingAsync(id);
        viewModel.AssignedEquipment = assignedEquipment.Select(e => e.ToListItem()).ToList();

        return View(viewModel);
    }

    public async Task<IActionResult> Create()
    {
        var categories = await categoryService.GetAllAsync();
        var viewModel = new MyListingCreateViewModel
        {
            Categories = categories.OrderBy(c => c.Name)
                .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name }).ToList()
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(MyListingCreateViewModel model)
    {
        if (!ModelState.IsValid)
        {
            var categories = await categoryService.GetAllAsync();
            model.Categories = categories.OrderBy(c => c.Name)
                .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name }).ToList();
            return View(model);
        }

        if (!TryGetUserId(out var userId)) return NotFound();

        var listing = await listingService.CreateAsync(userId, model.ToCreateListingDto());
        return RedirectToAction(nameof(Details), new { id = listing.Id });
    }

    public async Task<IActionResult> Edit(Guid id)
    {
        var profile = await GetProviderProfileAsync();
        if (profile == null) return NotFound();

        var listing = await listingService.GetByIdAsync(id);
        if (listing == null || listing.UserProfileId != profile.Id) return NotFound();

        var categories = await categoryService.GetAllAsync();
        var viewModel = new MyListingEditViewModel
        {
            Id = listing.Id,
            Title = listing.Title,
            Description = listing.Description,
            ServiceCategoryId = listing.ServiceCategoryId,
            PricePerHectare = listing.PricePerHectare,
            IsActive = listing.IsActive,
            Categories = categories.OrderBy(c => c.Name)
                .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name }).ToList()
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, MyListingEditViewModel model)
    {
        if (id != model.Id) return BadRequest();

        if (!ModelState.IsValid)
        {
            var categories = await categoryService.GetAllAsync();
            model.Categories = categories.OrderBy(c => c.Name)
                .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name }).ToList();
            return View(model);
        }

        if (!TryGetUserId(out var userId)) return NotFound();

        try
        {
            var listing = await listingService.UpdateAsync(userId, model.ToUpdateListingDto());
            return RedirectToAction(nameof(Details), new { id = listing.Id });
        }
        catch (BusinessRuleException)
        {
            return NotFound();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    public async Task<IActionResult> Delete(Guid id)
    {
        var profile = await GetProviderProfileAsync();
        if (profile == null) return NotFound();

        var listing = await listingService.GetByIdAsync(id);
        if (listing == null || listing.UserProfileId != profile.Id) return NotFound();

        var viewModel = listing.ToMyListingDetails(await bookingService.GetCountByListingAsync(id));
        viewModel.HasActiveBookings = await bookingService.HasActiveBookingsAsync(id);
        return View(viewModel);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        if (!TryGetUserId(out var userId)) return NotFound();

        try
        {
            await listingService.DeleteAsync(userId, id);
            return RedirectToAction(nameof(Index));
        }
        catch (BusinessRuleException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            var listing = await listingService.GetByIdAsync(id);
            if (listing == null) return NotFound();
            var deleteViewModel = listing.ToMyListingDetails(await bookingService.GetCountByListingAsync(id));
            deleteViewModel.HasActiveBookings = true;
            return View("Delete", deleteViewModel);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleActive(Guid id)
    {
        if (!TryGetUserId(out var userId)) return NotFound();

        try
        {
            await listingService.ToggleActiveAsync(userId, id);
        }
        catch (BusinessRuleException)
        {
            return NotFound();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    public async Task<IActionResult> Availabilities(Guid id)
    {
        var profile = await GetProviderProfileAsync();
        if (profile == null) return NotFound();

        var listing = await listingService.GetByIdAsync(id);
        if (listing == null || listing.UserProfileId != profile.Id) return NotFound();

        return View(listing.ToAvailabilitiesVm());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddAvailability(Guid listingId, ManageAvailabilitiesViewModel model)
    {
        var profile = await GetProviderProfileAsync();
        if (profile == null) return NotFound();

        var listing = await listingService.GetByIdAsync(listingId);
        if (listing == null || listing.UserProfileId != profile.Id) return NotFound();

        if (model.AddStartTime >= model.AddEndTime)
            ModelState.AddModelError(string.Empty, "Start time must be before end time.");

        if (!ModelState.IsValid)
        {
            var reload = await listingService.GetByIdAsync(listingId);
            if (reload == null) return NotFound();
            return View("Availabilities", reload.ToAvailabilitiesVm());
        }

        if (!TryGetUserId(out var userId)) return NotFound();

        try
        {
            await listingService.AddAvailabilityAsync(userId, new CreateAvailabilityDto
            {
                ListingId = listingId,
                StartTime = model.AddStartTime,
                EndTime = model.AddEndTime
            });
        }
        catch (BusinessRuleException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(Availabilities), new { id = listingId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteAvailability(Guid id)
    {
        var availability = await listingService.GetAvailabilityByIdAsync(id);
        if (availability == null) return NotFound();

        if (!TryGetUserId(out var userId)) return NotFound();

        try
        {
            await listingService.DeleteAvailabilityAsync(userId, id);
        }
        catch (BusinessRuleException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }

        return RedirectToAction(nameof(Availabilities), new { id = availability.ServiceListingId });
    }

    public async Task<IActionResult> Bookings(Guid id)
    {
        var profile = await GetProviderProfileAsync();
        if (profile == null) return NotFound();

        var listing = await listingService.GetByIdAsync(id);
        if (listing == null || listing.UserProfileId != profile.Id) return NotFound();

        var bookings = await bookingService.GetByListingAsync(id);
        var viewModel = new BookingsForListingViewModel
        {
            ListingId = listing.Id,
            ListingTitle = listing.Title,
            Bookings = bookings.Select(b => b.ToMyListingBookingItem()).ToList()
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateBookingStatus(Guid bookingId, Guid listingId, BookingStatus status)
    {
        var profile = await GetProviderProfileAsync();
        if (profile == null) return Unauthorized();

        var listing = await listingService.GetByIdAsync(listingId);
        if (listing == null || listing.UserProfileId != profile.Id) return NotFound();

        try
        {
            await bookingService.UpdateStatusAsync(bookingId, status, profile.Id);
        }
        catch (BusinessRuleException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(Bookings), new { id = listingId });
    }
}
