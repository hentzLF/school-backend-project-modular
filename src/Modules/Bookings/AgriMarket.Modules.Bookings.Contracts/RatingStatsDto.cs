namespace AgriMarket.Modules.Bookings.Contracts;

/// <summary>
/// Aggregate review statistics owned by the Bookings module. Consumed by the
/// Marketplace module (listing ratings) and the Users module (profile ratings).
/// </summary>
public sealed record RatingStatsDto(double AverageRating, int ReviewCount)
{
    public static readonly RatingStatsDto Empty = new(0, 0);
}
