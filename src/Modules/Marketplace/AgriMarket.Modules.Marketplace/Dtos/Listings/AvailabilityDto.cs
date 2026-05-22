namespace AgriMarket.Modules.Marketplace.Dtos.Listings;

internal sealed class AvailabilityDto
{
    public Guid Id { get; init; }
    public DateTime StartTime { get; init; }
    public DateTime EndTime { get; init; }
    public bool IsBooked { get; init; }
    public Guid ServiceListingId { get; init; }
}
