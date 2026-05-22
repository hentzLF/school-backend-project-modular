using System.Text.Json.Serialization;
using AgriMarket.Modules.Bookings.Enums;

namespace AgriMarket.Modules.Bookings.Dtos.Bookings;

public sealed class BookingDto
{
    public Guid Id { get; init; }
    public BookingStatus Status { get; init; }
    public decimal TotalPrice { get; init; }
    public decimal AreaInHectares { get; init; }
    public DateTime CreatedAt { get; init; }
    public string? Notes { get; init; }
    public Guid ServiceListingId { get; init; }
    public Guid ClientProfileId { get; init; }
    public Guid AvailabilityId { get; init; }

    public int? PaymentStatus { get; init; }
    public decimal? PaymentAmount { get; init; }
    public decimal? PaymentPlatformFee { get; init; }

    [JsonIgnore]
    public string ClientName { get; init; } = "Unknown";

    [JsonIgnore]
    public string ListingTitle { get; init; } = "Unknown";

    [JsonIgnore]
    public Guid ProviderProfileId { get; init; }

    [JsonIgnore]
    public DateTime AvailabilityStart { get; init; }

    [JsonIgnore]
    public DateTime AvailabilityEnd { get; init; }
}
