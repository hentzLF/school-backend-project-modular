namespace AgriMarket.Web.Areas.Client.ViewModels.Reviews;

internal class ReviewListViewModel
{
    public IEnumerable<ReviewViewModel> Reviews { get; set; } = [];
    public Guid ProfileId { get; set; }
    public string ProviderName { get; set; } = default!;
    public double AverageRating { get; set; }
    public int ReviewCount { get; set; }
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
}
