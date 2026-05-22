using AgriMarket.Web.Areas.Client.ViewModels.Reviews;

namespace AgriMarket.Web.Areas.Client.ViewModels.Listings;

internal class ListingIndexViewModel
{
    public IEnumerable<ListingIndexItemViewModel> Listings { get; set; } = [];
}

internal class ListingIndexItemViewModel
{
    public Guid Id { get; set; }
    public string Title { get; set; } = default!;
    public string CategoryName { get; set; } = default!;
    public string ProviderName { get; set; } = default!;
    public decimal PricePerHectare { get; set; }
    public bool IsActive { get; set; }
    public RatingStatsViewModel? RatingStats { get; set; }
}
