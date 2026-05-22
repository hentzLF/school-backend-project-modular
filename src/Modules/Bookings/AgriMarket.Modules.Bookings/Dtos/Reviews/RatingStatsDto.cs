namespace AgriMarket.Modules.Bookings.Dtos.Reviews;

/// <summary>Module-internal rating projection used by review queries.</summary>
internal sealed class RatingStatsDto
{
    public double AverageRating { get; init; }
    public int ReviewCount { get; init; }
}
