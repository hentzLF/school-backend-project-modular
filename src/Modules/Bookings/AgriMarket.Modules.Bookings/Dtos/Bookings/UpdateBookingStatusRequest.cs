using System.ComponentModel.DataAnnotations;
using AgriMarket.Modules.Bookings.Enums;

namespace AgriMarket.Modules.Bookings.Dtos.Bookings;

public sealed class UpdateBookingStatusRequest
{
    [Required]
    [EnumDataType(typeof(BookingStatus))]
    public BookingStatus Status { get; init; }
}
