using System.ComponentModel.DataAnnotations;
using AgriMarket.Modules.Marketplace.Enums;

namespace AgriMarket.Modules.Marketplace.Dtos.Equipment;

public sealed class UpdateEquipmentStatusDto
{
    [Required]
    [EnumDataType(typeof(EquipmentStatus))]
    public EquipmentStatus Status { get; init; }
}
