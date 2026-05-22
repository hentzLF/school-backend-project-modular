using System.ComponentModel.DataAnnotations;

namespace AgriMarket.Web.Areas.Client.ViewModels.Reviews;

internal class EditReviewViewModel
{
    [Required]
    public Guid ReviewId { get; set; }

    [Required]
    public Guid BookingId { get; set; }

    [Range(1, 5)]
    public int Rating { get; set; }

    [MaxLength(2000)]
    public string? Comment { get; set; }

    public string BookingTitle { get; set; } = default!;
}
