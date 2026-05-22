using AgriMarket.Modules.Bookings.Enums;

namespace AgriMarket.Modules.Bookings.Entities;

public sealed class Booking
{
    public Guid Id { get; set; }

    public BookingStatus Status { get; set; }

    public decimal TotalPrice { get; set; }

    public decimal AreaInHectares { get; set; }

    public DateTime CreatedAt { get; set; }

    public string? Notes { get; set; }

    // Foreign Keys (cross-module — plain Guid columns, no navigation properties)
    public Guid ServiceListingId { get; set; }

    public Guid ClientProfileId { get; set; }

    public Guid AvailabilityId { get; set; }

    // Navigation (in-module only)
    public Payment? Payment { get; set; }
    public Review? Review { get; set; }
}
