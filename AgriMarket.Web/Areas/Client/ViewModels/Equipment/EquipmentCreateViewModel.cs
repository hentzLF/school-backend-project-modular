using System.ComponentModel.DataAnnotations;
using AgriMarket.Modules.Marketplace.Enums;

namespace AgriMarket.Web.Areas.Client.ViewModels.Equipment;

public class EquipmentCreateViewModel
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = default!;

    [Required]
    [MaxLength(200)]
    public string Make { get; set; } = default!;

    [MaxLength(200)]
    public string? Model { get; set; }

    [Range(1900, 2100)]
    public int? ManufactureYear { get; set; }

    [Range(0, 10000)]
    public int? HorsePower { get; set; }

    [Required]
    [EnumDataType(typeof(EquipmentCondition))]
    public EquipmentCondition Condition { get; set; }

    [MaxLength(2000)]
    public string? Description { get; set; }
}
