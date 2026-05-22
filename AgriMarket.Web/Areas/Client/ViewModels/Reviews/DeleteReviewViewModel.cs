namespace AgriMarket.Web.Areas.Client.ViewModels.Reviews;

internal class DeleteReviewViewModel
{
    public Guid ReviewId { get; set; }
    public Guid BookingId { get; set; }
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public string ReviewerName { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
}
