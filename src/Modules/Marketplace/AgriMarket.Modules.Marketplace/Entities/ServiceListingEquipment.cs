namespace AgriMarket.Modules.Marketplace.Entities;

public sealed class ServiceListingEquipment
{
    public Guid ServiceListingId { get; set; }
    public Guid EquipmentId { get; set; }

    // Navigation
    public ServiceListing? ServiceListing { get; set; }
    public Equipment? Equipment { get; set; }
}
