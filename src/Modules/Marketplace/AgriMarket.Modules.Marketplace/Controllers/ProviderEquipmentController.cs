using AgriMarket.Modules.Marketplace.Dtos.Equipment;
using AgriMarket.Modules.Marketplace.Services;
using AgriMarket.Shared.Exceptions;
using AgriMarket.Shared.Web;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgriMarket.Modules.Marketplace.Controllers;

[ApiController]
[ApiVersion("1")]
[Route("api/v{version:apiVersion}/provider/equipment")]
[Authorize]
internal sealed class ProviderEquipmentController(IEquipmentService equipmentService) : ApiControllerBase
{
    private readonly IEquipmentService _equipmentService = equipmentService;

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<EquipmentDto>), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        if (!TryGetProfileId(out var profileId))
            return Problem(statusCode: 403, title: "Forbidden", detail: "No provider profile found.");

        var result = await _equipmentService.GetByProviderAsync(profileId, ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(EquipmentDto), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        if (!TryGetProfileId(out var profileId))
            return Problem(statusCode: 403, title: "Forbidden", detail: "No provider profile found.");

        var result = await _equipmentService.GetByIdAsync(profileId, id, ct);
        if (result is null)
            return Problem(statusCode: 404, title: "Not Found", detail: $"Equipment {id} not found.");

        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(EquipmentDto), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<IActionResult> Create([FromBody] CreateEquipmentDto dto, CancellationToken ct)
    {
        if (!TryGetProfileId(out var profileId))
            return Problem(statusCode: 403, title: "Forbidden", detail: "No provider profile found.");

        var result = await _equipmentService.CreateAsync(profileId, dto, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(EquipmentDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateEquipmentDto dto, CancellationToken ct)
    {
        if (!TryGetProfileId(out var profileId))
            return Problem(statusCode: 403, title: "Forbidden", detail: "No provider profile found.");

        try
        {
            var result = await _equipmentService.UpdateAsync(profileId, id, dto, ct);
            return Ok(result);
        }
        catch (KeyNotFoundException)
        {
            return Problem(statusCode: 404, title: "Not Found", detail: $"Equipment {id} not found.");
        }
        catch (BusinessRuleException ex)
        {
            return Problem(statusCode: 403, title: "Forbidden", detail: ex.Message);
        }
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        if (!TryGetProfileId(out var profileId))
            return Problem(statusCode: 403, title: "Forbidden", detail: "No provider profile found.");

        try
        {
            await _equipmentService.DeleteAsync(profileId, id, ct);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return Problem(statusCode: 404, title: "Not Found", detail: $"Equipment {id} not found.");
        }
        catch (BusinessRuleException ex)
        {
            return Problem(statusCode: 403, title: "Forbidden", detail: ex.Message);
        }
    }

    [HttpPatch("{id:guid}/status")]
    [ProducesResponseType(typeof(EquipmentDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateEquipmentStatusDto dto, CancellationToken ct)
    {
        if (!TryGetProfileId(out var profileId))
            return Problem(statusCode: 403, title: "Forbidden", detail: "No provider profile found.");

        try
        {
            var result = await _equipmentService.UpdateStatusAsync(profileId, id, dto.Status, ct);
            return Ok(result);
        }
        catch (KeyNotFoundException)
        {
            return Problem(statusCode: 404, title: "Not Found", detail: $"Equipment {id} not found.");
        }
        catch (BusinessRuleException ex)
        {
            return Problem(statusCode: 403, title: "Forbidden", detail: ex.Message);
        }
    }
}
