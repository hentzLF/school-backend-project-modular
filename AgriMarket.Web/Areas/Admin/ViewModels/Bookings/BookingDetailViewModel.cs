using AgriMarket.Modules.Bookings.Enums;

namespace AgriMarket.Web.Areas.Admin.ViewModels;

public class BookingDetailViewModel
{
    public Guid Id { get; set; }
    public BookingStatus Status { get; set; }
    public decimal TotalPrice { get; set; }
    public decimal AreaInHectares { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? Notes { get; set; }
    public string ClientName { get; set; } = default!;
    public Guid ClientProfileId { get; set; }
    public string ListingTitle { get; set; } = default!;
    public Guid ListingId { get; set; }
    public DateTime AvailabilityStart { get; set; }
    public DateTime AvailabilityEnd { get; set; }
    public Guid? PaymentId { get; set; }
    public decimal? PaymentAmount { get; set; }
    public decimal? PlatformFee { get; set; }
    public PaymentStatus? PaymentStatus { get; set; }
    public int? ReviewRating { get; set; }
    public string? ReviewComment { get; set; }
    public DateTime? ReviewCreatedAt { get; set; }
}
