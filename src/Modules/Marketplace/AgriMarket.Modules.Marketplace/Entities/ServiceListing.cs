namespace AgriMarket.Modules.Marketplace.Entities;

internal sealed class ServiceListing
{
    public Guid Id { get; set; }

    public string Title { get; set; } = default!;

    public string? Description { get; set; }

    public decimal PricePerHectare { get; set; }

    public bool IsActive { get; set; }

    // Foreign Keys
    public Guid UserProfileId { get; set; }
    public Guid ServiceCategoryId { get; set; }
    public Guid? LocationId { get; set; }

    // Navigation (in-module only — UserProfile nav removed)
    public ServiceCategory? ServiceCategory { get; set; }
    public Location? Location { get; set; }
    public ICollection<ServiceListingEquipment>? ServiceListingEquipments { get; set; }
    public ICollection<Availability>? Availabilities { get; set; }
}
