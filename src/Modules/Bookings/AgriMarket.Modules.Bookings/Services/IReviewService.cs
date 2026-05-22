using AgriMarket.Modules.Bookings.Dtos.Reviews;

namespace AgriMarket.Modules.Bookings.Services;

internal interface IReviewService
{
    Task<ReviewDto?> GetByBookingAsync(Guid bookingId, CancellationToken ct = default);

    /// <param name="reviewerProfileId">UserProfile id of the reviewing client.</param>
    Task<ReviewDto> CreateAsync(Guid reviewerProfileId, CreateReviewDto dto, CancellationToken ct = default);

    Task<(IEnumerable<ReviewDto> Items, int TotalCount)> GetAllAsync(int page, int pageSize, CancellationToken ct = default);
    Task<ReviewDto?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <param name="reviewerProfileId">UserProfile id of the review owner.</param>
    Task<ReviewDto> UpdateAsync(Guid reviewerProfileId, UpdateReviewDto dto, CancellationToken ct = default);

    /// <param name="reviewerProfileId">UserProfile id of the review owner.</param>
    Task DeleteAsync(Guid reviewerProfileId, Guid reviewId, CancellationToken ct = default);

    Task<(IEnumerable<ReviewDto> Items, int TotalCount)> GetByProfileAsync(Guid profileId, int page, int pageSize, CancellationToken ct = default);
    Task<RatingStatsDto> GetRatingStatsForProfileAsync(Guid profileId, CancellationToken ct = default);
    Task<RatingStatsDto> GetRatingStatsForListingAsync(Guid listingId, CancellationToken ct = default);
}
