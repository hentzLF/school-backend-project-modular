using AgriMarket.Modules.Bookings.Enums;

namespace AgriMarket.Modules.Bookings.Dtos.Bookings;

public sealed class BookingSummaryDto
{
    public Guid Id { get; init; }
    public string ClientName { get; init; } = default!;
    public BookingStatus Status { get; init; }
    public decimal AreaInHectares { get; init; }
    public decimal TotalPrice { get; init; }
    public DateTime CreatedAt { get; init; }
    public int? PaymentStatus { get; init; }
}
