using AgriMarket.Modules.Marketplace.Services;
using AgriMarket.Web.Areas.Admin.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgriMarket.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = "AdminOnly")]
public class CategoriesController : Controller
{
    private readonly ICategoryService _categoryService;

    public CategoriesController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    public async Task<IActionResult> Index()
    {
        var categories = await _categoryService.GetAllAsync();
        var listingCounts = await _categoryService.GetListingCountsAsync();

        var vm = new CategoryListViewModel
        {
            TotalCount = categories.Count(),
            Categories = categories.Select(c => new CategoryListItemViewModel
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                ListingsCount = listingCounts.GetValueOrDefault(c.Id, 0)
            })
        };

        return View(vm);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new CategoryCreateViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CategoryCreateViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);

        var category = new AgriMarket.Modules.Marketplace.Entities.ServiceCategory
        {
            Id = Guid.NewGuid(),
            Name = vm.Name,
            Description = vm.Description
        };

        await _categoryService.CreateAsync(category);

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(Guid id)
    {
        var category = await _categoryService.GetByIdAsync(id);
        if (category == null) return NotFound();

        var vm = new CategoryEditViewModel
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(CategoryEditViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);

        var category = await _categoryService.GetByIdAsync(vm.Id);
        if (category == null) return NotFound();

        category.Name = vm.Name;
        category.Description = vm.Description;
        await _categoryService.UpdateAsync(category);

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Delete(Guid id)
    {
        var category = await _categoryService.GetByIdAsync(id);
        if (category == null) return NotFound();

        var listingsCount = await _categoryService.GetListingCountAsync(id);

        var vm = new CategoryListItemViewModel
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            ListingsCount = listingsCount
        };

        return View(vm);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(Guid id)
    {
        var listingsCount = await _categoryService.GetListingCountAsync(id);
        if (listingsCount > 0)
        {
            ModelState.AddModelError(string.Empty, "Cannot delete category with existing listings");
            var category = await _categoryService.GetByIdAsync(id);
            return View("Delete", new CategoryListItemViewModel
            {
                Id = id,
                Name = category?.Name ?? string.Empty,
                Description = category?.Description,
                ListingsCount = listingsCount
            });
        }

        await _categoryService.DeleteAsync(id);

        return RedirectToAction(nameof(Index));
    }
}
