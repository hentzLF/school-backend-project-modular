namespace AgriMarket.Web.Areas.Admin.ViewModels;

internal class ListingListViewModel
{
    public IEnumerable<ListingListItemViewModel> Listings { get; set; } = [];
    public int TotalCount { get; set; }
    public bool? FilterActive { get; set; }
}
