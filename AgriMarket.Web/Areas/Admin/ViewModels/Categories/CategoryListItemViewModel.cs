namespace AgriMarket.Web.Areas.Admin.ViewModels;

internal class CategoryListItemViewModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public int ListingsCount { get; set; }
}
