using AgriMarket.Modules.Marketplace.Enums;

namespace AgriMarket.Modules.Marketplace.Entities;

internal sealed class Equipment
{
    public Guid Id { get; set; }

    public Guid UserProfileId { get; set; }

    public string Name { get; set; } = default!;

    public string Make { get; set; } = default!;

    public string? Model { get; set; }

    public int? ManufactureYear { get; set; }

    public int? HorsePower { get; set; }

    public EquipmentCondition Condition { get; set; }

    public EquipmentStatus Status { get; set; }

    public string? Description { get; set; }

    // Navigation (in-module only — UserProfile nav removed)
    public ICollection<ServiceListingEquipment>? ServiceListingEquipments { get; set; }
}
