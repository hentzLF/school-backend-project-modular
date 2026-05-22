namespace AgriMarket.Modules.Bookings.Entities;

public sealed class Review
{
    public Guid Id { get; set; }

    public int Rating { get; set; }

    public string? Comment { get; set; }

    public DateTime CreatedAt { get; set; }

    // FK
    public Guid BookingId { get; set; }

    // Cross-module FKs — plain Guid columns, no navigation properties
    public Guid ReviewerProfileId { get; set; }
    public Guid ReviewedProfileId { get; set; }

    // Navigation (in-module)
    public Booking? Booking { get; set; }
}
