using AgriMarket.Modules.Bookings.Enums;

namespace AgriMarket.Web.Areas.Client.ViewModels.Bookings;

internal class BookingIndexItemViewModel
{
    public Guid Id { get; set; }
    public string ListingTitle { get; set; } = default!;
    public BookingStatus Status { get; set; }
    public decimal TotalPrice { get; set; }
    public decimal AreaInHectares { get; set; }
    public DateTime CreatedAt { get; set; }
}
