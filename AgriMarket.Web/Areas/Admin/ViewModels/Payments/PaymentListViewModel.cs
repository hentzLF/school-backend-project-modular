using AgriMarket.Modules.Bookings.Enums;

namespace AgriMarket.Web.Areas.Admin.ViewModels;

internal class PaymentListViewModel
{
    public IEnumerable<PaymentListItemViewModel> Payments { get; set; } = [];
    public int TotalCount { get; set; }
    public PaymentStatus? FilterStatus { get; set; }
}
