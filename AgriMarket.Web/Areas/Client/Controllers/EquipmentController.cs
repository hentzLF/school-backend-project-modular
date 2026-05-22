using AgriMarket.Shared.Exceptions;
using AgriMarket.Modules.Users.Services;
using AgriMarket.Modules.Marketplace.Services;
using AgriMarket.Modules.Marketplace.Enums;
using AgriMarket.Web.Areas.Client.ViewModels.Equipment;
using AgriMarket.Web.Mappers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AgriMarket.Web.Areas.Client.Controllers;

[Area("Client")]
[Authorize(Policy = "ClientOnly")]
internal class EquipmentController(
    IEquipmentService equipmentService,
    IListingService listingService,
    IUserService userService) : Controller
{
    public async Task<IActionResult> Index()
    {
        var profileId = await GetProviderProfileIdAsync();
        if (profileId == null) return NotFound();

        var equipment = await equipmentService.GetByProviderAsync(profileId.Value);
        var viewModel = new EquipmentIndexViewModel
        {
            Equipments = equipment.Select(e => e.ToListItem()).ToList()
        };

        return View(viewModel);
    }

    public IActionResult Create()
    {
        return View(new EquipmentCreateViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(EquipmentCreateViewModel input)
    {
        if (!ModelState.IsValid)
            return View(input);

        var profileId = await GetProviderProfileIdAsync();
        if (profileId == null) return NotFound();

        await equipmentService.CreateAsync(profileId.Value, input.ToCreateDto());
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(Guid id)
    {
        var profileId = await GetProviderProfileIdAsync();
        if (profileId == null) return NotFound();

        var equipment = await equipmentService.GetByIdAsync(profileId.Value, id);
        if (equipment == null) return NotFound();

        return View(equipment.ToEditViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, EquipmentEditViewModel input)
    {
        if (id != input.Id) return BadRequest();

        if (!ModelState.IsValid)
            return View(input);

        var profileId = await GetProviderProfileIdAsync();
        if (profileId == null) return NotFound();

        try
        {
            await equipmentService.UpdateAsync(profileId.Value, id, input.ToUpdateDto());
            return RedirectToAction(nameof(Index));
        }
        catch (BusinessRuleException)
        {
            return NotFound();
        }
    }

    public async Task<IActionResult> Delete(Guid id)
    {
        var profileId = await GetProviderProfileIdAsync();
        if (profileId == null) return NotFound();

        var equipment = await equipmentService.GetByIdAsync(profileId.Value, id);
        if (equipment == null) return NotFound();

        return View(equipment.ToDeleteViewModel());
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        var profileId = await GetProviderProfileIdAsync();
        if (profileId == null) return NotFound();

        try
        {
            await equipmentService.DeleteAsync(profileId.Value, id);
            return RedirectToAction(nameof(Index));
        }
        catch (BusinessRuleException)
        {
            return NotFound();
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(Guid id, EquipmentStatus status)
    {
        var profileId = await GetProviderProfileIdAsync();
        if (profileId == null) return NotFound();

        try
        {
            await equipmentService.UpdateStatusAsync(profileId.Value, id, status);
            return RedirectToAction(nameof(Index));
        }
        catch (BusinessRuleException)
        {
            return NotFound();
        }
    }

    public async Task<IActionResult> AssignToListing(Guid listingId)
    {
        var profileId = await GetProviderProfileIdAsync();
        if (profileId == null) return NotFound();

        var listing = await listingService.GetByIdAsync(listingId);
        if (listing == null || listing.UserProfileId != profileId.Value) return NotFound();

        var providerEquipment = await equipmentService.GetByProviderAsync(profileId.Value);
        var assignedEquipment = await equipmentService.GetByListingAsync(listingId);
        var assignedIds = assignedEquipment.Select(e => e.Id).ToHashSet();

        var viewModel = new EquipmentAssignViewModel
        {
            ListingId = listingId,
            ListingTitle = listing.Title,
            Equipment = providerEquipment.Select(e => new EquipmentAssignItemViewModel
            {
                EquipmentId = e.Id,
                Name = e.Name,
                Make = e.Make,
                Model = e.Model,
                Status = e.Status.ToString(),
                IsSelected = assignedIds.Contains(e.Id)
            }).ToList()
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AssignToListing(Guid listingId, List<Guid> selectedEquipmentIds)
    {
        var profileId = await GetProviderProfileIdAsync();
        if (profileId == null) return NotFound();

        var listing = await listingService.GetByIdAsync(listingId);
        if (listing == null || listing.UserProfileId != profileId.Value) return NotFound();

        await equipmentService.AssignToListingAsync(profileId.Value, listingId, selectedEquipmentIds);
        return RedirectToAction("Details", "MyListings", new { id = listingId });
    }

    private async Task<Guid?> GetProviderProfileIdAsync()
    {
        if (!Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
            return null;

        var profile = await userService.GetProfileByUserIdAsync(userId);
        return profile?.Id;
    }
}
