namespace AgriMarket.Web.Areas.Client.ViewModels.Payments;

internal class PaymentHistoryViewModel
{
    public IEnumerable<PaymentHistoryItemViewModel> Payments { get; set; } = [];
}
