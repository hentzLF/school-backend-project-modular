using AgriMarket.Modules.Bookings.Enums;

namespace AgriMarket.Web.Areas.Client.ViewModels.Payments;

internal class CheckoutViewModel
{
    public Guid BookingId { get; set; }
    public string ListingTitle { get; set; } = default!;
    public decimal AreaInHectares { get; set; }
    public decimal ServiceTotal { get; set; }
    public decimal PlatformFee { get; set; }
    public decimal GrandTotal { get; set; }
    public PaymentMethod SelectedMethod { get; set; } = PaymentMethod.Card;
}
