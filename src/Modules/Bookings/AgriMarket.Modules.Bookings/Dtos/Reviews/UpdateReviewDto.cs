using System.ComponentModel.DataAnnotations;

namespace AgriMarket.Modules.Bookings.Dtos.Reviews;

internal sealed class UpdateReviewDto
{
    public Guid Id { get; init; }

    [Range(1, 5)]
    public int Rating { get; init; }

    [MaxLength(2000)]
    public string? Comment { get; init; }
}
