using AgriMarket.Modules.Bookings.Enums;

namespace AgriMarket.Web.Areas.Admin.ViewModels;

internal class PaymentDetailViewModel
{
    public Guid Id { get; set; }
    public decimal Amount { get; set; }
    public decimal PlatformFee { get; set; }
    public PaymentStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ReleasedAt { get; set; }
    public Guid BookingId { get; set; }
    public BookingStatus BookingStatus { get; set; }
    public Guid ListingId { get; set; }
    public string ListingTitle { get; set; } = default!;
    public string ClientName { get; set; } = default!;
    public Guid ClientProfileId { get; set; }
    public string ProviderName { get; set; } = default!;
    public Guid ProviderProfileId { get; set; }
}
