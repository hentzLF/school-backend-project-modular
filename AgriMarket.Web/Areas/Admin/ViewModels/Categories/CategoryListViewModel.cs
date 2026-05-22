namespace AgriMarket.Web.Areas.Admin.ViewModels;

internal class CategoryListViewModel
{
    public IEnumerable<CategoryListItemViewModel> Categories { get; set; } = [];
    public int TotalCount { get; set; }
}
