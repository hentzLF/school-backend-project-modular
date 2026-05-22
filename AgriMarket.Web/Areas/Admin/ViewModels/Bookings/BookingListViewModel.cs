using AgriMarket.Modules.Bookings.Enums;

namespace AgriMarket.Web.Areas.Admin.ViewModels;

public class BookingListViewModel
{
    public IEnumerable<BookingListItemViewModel> Bookings { get; set; } = [];
    public int TotalCount { get; set; }
    public BookingStatus? FilterStatus { get; set; }
}
