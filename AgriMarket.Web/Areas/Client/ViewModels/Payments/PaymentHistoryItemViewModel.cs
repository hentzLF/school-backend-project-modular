namespace AgriMarket.Web.Areas.Client.ViewModels.Payments;

internal class PaymentHistoryItemViewModel
{
    public Guid PaymentId { get; set; }
    public Guid BookingId { get; set; }
    public string ListingTitle { get; set; } = default!;
    public decimal Amount { get; set; }
    public decimal PlatformFee { get; set; }
    public string Method { get; set; } = default!;
    public string Status { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
    public DateTime? ReleasedAt { get; set; }
}
