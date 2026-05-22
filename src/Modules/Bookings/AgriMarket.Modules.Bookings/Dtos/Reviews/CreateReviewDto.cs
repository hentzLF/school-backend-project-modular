using System.ComponentModel.DataAnnotations;

namespace AgriMarket.Modules.Bookings.Dtos.Reviews;

public sealed class CreateReviewDto
{
    [Required]
    public Guid BookingId { get; init; }

    [Range(1, 5)]
    public int Rating { get; init; }

    [MaxLength(2000)]
    public string? Comment { get; init; }
}
