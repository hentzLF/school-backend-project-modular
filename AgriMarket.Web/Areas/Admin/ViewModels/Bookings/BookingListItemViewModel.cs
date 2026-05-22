using AgriMarket.Modules.Bookings.Enums;

namespace AgriMarket.Web.Areas.Admin.ViewModels;

public class BookingListItemViewModel
{
    public Guid Id { get; set; }
    public string ClientName { get; set; } = default!;
    public string ListingTitle { get; set; } = default!;
    public BookingStatus Status { get; set; }
    public decimal TotalPrice { get; set; }
    public decimal AreaInHectares { get; set; }
    public DateTime CreatedAt { get; set; }
}
