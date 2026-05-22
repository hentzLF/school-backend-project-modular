using AgriMarket.Modules.Bookings.Enums;

namespace AgriMarket.Web.Areas.Admin.ViewModels;

public class PaymentListItemViewModel
{
    public Guid Id { get; set; }
    public Guid BookingId { get; set; }
    public decimal Amount { get; set; }
    public decimal PlatformFee { get; set; }
    public PaymentStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ReleasedAt { get; set; }
}
