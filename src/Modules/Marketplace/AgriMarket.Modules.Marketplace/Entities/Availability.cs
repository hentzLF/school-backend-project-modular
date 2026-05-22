namespace AgriMarket.Modules.Marketplace.Entities;

internal sealed class Availability
{
    public Guid Id { get; set; }

    public Guid ServiceListingId { get; set; }

    public DateTime StartTime { get; set; }

    public DateTime EndTime { get; set; }

    public bool IsBooked { get; set; }

    public uint RowVersion { get; set; }

    // Navigation
    public ServiceListing? ServiceListing { get; set; }
}
