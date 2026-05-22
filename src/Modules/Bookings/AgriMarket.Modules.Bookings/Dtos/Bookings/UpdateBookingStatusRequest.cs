using System.ComponentModel.DataAnnotations;
using AgriMarket.Modules.Bookings.Enums;

namespace AgriMarket.Modules.Bookings.Dtos.Bookings;

internal sealed class UpdateBookingStatusRequest
{
    [Required]
    [EnumDataType(typeof(BookingStatus))]
    public BookingStatus Status { get; init; }
}
