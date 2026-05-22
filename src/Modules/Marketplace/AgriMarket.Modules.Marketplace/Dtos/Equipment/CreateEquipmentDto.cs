using System.ComponentModel.DataAnnotations;
using AgriMarket.Modules.Marketplace.Enums;

namespace AgriMarket.Modules.Marketplace.Dtos.Equipment;

public sealed class CreateEquipmentDto
{
    [Required]
    [MaxLength(200)]
    public string Name { get; init; } = default!;

    [Required]
    [MaxLength(200)]
    public string Make { get; init; } = default!;

    [MaxLength(200)]
    public string? Model { get; init; }

    [Range(1900, 2100)]
    public int? ManufactureYear { get; init; }

    [Range(0, 10000)]
    public int? HorsePower { get; init; }

    [Required]
    [EnumDataType(typeof(EquipmentCondition))]
    public EquipmentCondition Condition { get; init; }

    [MaxLength(2000)]
    public string? Description { get; init; }
}
