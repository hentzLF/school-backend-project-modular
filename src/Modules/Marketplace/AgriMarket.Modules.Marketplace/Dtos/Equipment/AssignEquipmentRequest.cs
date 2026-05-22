using System.ComponentModel.DataAnnotations;

namespace AgriMarket.Modules.Marketplace.Dtos.Equipment;

public sealed class AssignEquipmentRequest
{
    [Required]
    public IReadOnlyList<Guid> EquipmentIds { get; init; } = [];
}
