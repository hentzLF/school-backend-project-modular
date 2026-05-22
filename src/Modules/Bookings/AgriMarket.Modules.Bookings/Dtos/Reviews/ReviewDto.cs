namespace AgriMarket.Modules.Bookings.Dtos.Reviews;

public sealed class ReviewDto
{
    public Guid Id { get; init; }
    public int Rating { get; init; }
    public string? Comment { get; init; }
    public DateTime CreatedAt { get; init; }
    public Guid BookingId { get; init; }
    public Guid ReviewerProfileId { get; init; }
    public Guid ReviewedProfileId { get; init; }
}
