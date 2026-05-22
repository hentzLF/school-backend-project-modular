using AgriMarket.Modules.Marketplace.Dtos.Categories;
using AgriMarket.Modules.Marketplace.Entities;
using AgriMarket.Modules.Marketplace.Services;
using AgriMarket.Shared.Web;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgriMarket.Modules.Marketplace.Controllers.Admin;

[ApiController]
[ApiVersion("1")]
[Route("api/v{version:apiVersion}/admin/categories")]
[Authorize(Policy = "AdminOnly")]
internal sealed class AdminCategoriesController(ICategoryService categoryService) : ApiControllerBase
{
    private readonly ICategoryService _categoryService = categoryService;

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<CategoryDto>), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<IActionResult> GetAll()
    {
        var categories = await _categoryService.GetAllAsync();
        var result = categories.Select(c => new CategoryDto
        {
            Id = c.Id,
            Name = c.Name,
            Description = c.Description
        });
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(CategoryDto), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var category = await _categoryService.GetByIdAsync(id);
        if (category is null)
            return Problem(statusCode: 404, title: "Not Found", detail: $"Category {id} not found.");

        return Ok(new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description
        });
    }

    [HttpPost]
    [ProducesResponseType(typeof(CategoryDto), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<IActionResult> Create([FromBody] CreateCategoryDto dto)
    {
        var category = new ServiceCategory
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Description = dto.Description
        };

        await _categoryService.CreateAsync(category);

        var result = new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description
        };
        return CreatedAtAction(nameof(GetById), new { id = category.Id }, result);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(CategoryDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCategoryDto dto)
    {
        var existing = await _categoryService.GetByIdAsync(id);
        if (existing is null)
            return Problem(statusCode: 404, title: "Not Found", detail: $"Category {id} not found.");

        var category = new ServiceCategory
        {
            Id = id,
            Name = dto.Name,
            Description = dto.Description
        };

        await _categoryService.UpdateAsync(category);

        var result = new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description
        };
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    [ProducesResponseType(422)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var existing = await _categoryService.GetByIdAsync(id);
        if (existing is null)
            return Problem(statusCode: 404, title: "Not Found", detail: $"Category {id} not found.");

        var listingCount = await _categoryService.GetListingCountAsync(id);
        if (listingCount > 0)
            return Problem(statusCode: 422, title: "Unprocessable Entity",
                detail: $"Cannot delete category with {listingCount} associated listing(s).");

        await _categoryService.DeleteAsync(id);
        return NoContent();
    }
}
