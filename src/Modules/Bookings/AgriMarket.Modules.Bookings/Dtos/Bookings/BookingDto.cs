using AgriMarket.Modules.Bookings.Enums;

namespace AgriMarket.Modules.Bookings.Dtos.Bookings;

internal sealed class BookingDto
{
    public Guid Id { get; init; }
    public BookingStatus Status { get; init; }
    public decimal TotalPrice { get; init; }
    public decimal AreaInHectares { get; init; }
    public DateTime CreatedAt { get; init; }
    public string? Notes { get; init; }
    public Guid ServiceListingId { get; init; }
    public Guid ClientProfileId { get; init; }
    public Guid ProviderProfileId { get; init; }
    public Guid AvailabilityId { get; init; }
    public DateTime? AvailabilityStart { get; init; }
    public DateTime? AvailabilityEnd { get; init; }
    public string ClientName { get; init; } = "Unknown";
    public string ListingTitle { get; init; } = "Unknown";

    public int? PaymentStatus { get; init; }
    public decimal? PaymentAmount { get; init; }
    public decimal? PaymentPlatformFee { get; init; }
}
