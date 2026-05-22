using AgriMarket.Modules.Bookings.Enums;

namespace AgriMarket.Modules.Bookings.Entities;

internal sealed class Payment
{
    public Guid Id { get; set; }

    public PaymentStatus Status { get; set; }

    public PaymentMethod Method { get; set; }

    public decimal Amount { get; set; }

    public decimal PlatformFee { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? ReleasedAt { get; set; }

    // FK
    public Guid BookingId { get; set; }

    // Navigation (in-module)
    public Booking? Booking { get; set; }
}
