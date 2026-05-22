using System.ComponentModel.DataAnnotations;

namespace AgriMarket.Modules.Marketplace.Dtos.Equipment;

internal sealed class AssignEquipmentRequest
{
    [Required]
    public IReadOnlyList<Guid> EquipmentIds { get; init; } = [];
}
