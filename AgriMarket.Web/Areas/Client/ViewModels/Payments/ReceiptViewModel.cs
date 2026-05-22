namespace AgriMarket.Web.Areas.Client.ViewModels.Payments;

internal class ReceiptViewModel
{
    public Guid PaymentId { get; set; }
    public Guid BookingId { get; set; }
    public decimal Amount { get; set; }
    public decimal PlatformFee { get; set; }
    public decimal TotalCharged { get; set; }
    public string Method { get; set; } = default!;
    public string Status { get; set; } = default!;
    public DateTime PaidAt { get; set; }
}
