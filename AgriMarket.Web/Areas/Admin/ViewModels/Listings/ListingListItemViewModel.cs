namespace AgriMarket.Web.Areas.Admin.ViewModels;

internal class ListingListItemViewModel
{
    public Guid Id { get; set; }
    public string Title { get; set; } = default!;
    public string ProviderName { get; set; } = default!;
    public string CategoryName { get; set; } = default!;
    public decimal PricePerHectare { get; set; }
    public bool IsActive { get; set; }
}
