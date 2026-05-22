using AgriMarket.Modules.Bookings.Dtos.Reviews;
using AgriMarket.Modules.Bookings.Entities;
using AgriMarket.Modules.Bookings.Enums;
using AgriMarket.Modules.Bookings.Persistence;
using AgriMarket.Modules.Marketplace.Contracts;
using AgriMarket.Shared.Exceptions;
using AgriMarket.Shared.Persistence;

namespace AgriMarket.Modules.Bookings.Services;

internal sealed class ReviewService(
    IRepository<Review> reviews,
    IBookingRepository bookings,
    IUnitOfWork uow,
    IQueryMaterializer mat,
    ICatalogModule catalog) : IReviewService
{
    public async Task<ReviewDto?> GetByBookingAsync(Guid bookingId, CancellationToken ct = default)
    {
        var review = await reviews.FirstOrDefaultAsync(r => r.BookingId == bookingId, ct);
        return review is null ? null : ToReviewDto(review);
    }

    public async Task<(IEnumerable<ReviewDto> Items, int TotalCount)> GetAllAsync(
        int page, int pageSize, CancellationToken ct = default)
    {
        var query = reviews.Query();
        var totalCount = await mat.CountAsync(query, ct);
        var items = await mat.ToListAsync(
            query.OrderByDescending(r => r.CreatedAt).Skip((page - 1) * pageSize).Take(pageSize), ct);
        return (items.Select(ToReviewDto), totalCount);
    }

    public async Task<ReviewDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var review = await reviews.FirstOrDefaultAsync(r => r.Id == id, ct);
        return review is null ? null : ToReviewDto(review);
    }

    public async Task<ReviewDto> CreateAsync(Guid reviewerProfileId, CreateReviewDto dto, CancellationToken ct = default)
    {
        var booking = await bookings.GetByIdWithDetailsAsync(dto.BookingId, ct)
            ?? throw new KeyNotFoundException("Booking not found.");

        if (booking.ClientProfileId != reviewerProfileId)
            throw new BusinessRuleException("Only the client can review a booking.");

        if (booking.Status != BookingStatus.ClientConfirmed && booking.Status != BookingStatus.ProviderCompleted)
            throw new BusinessRuleException("Cannot review a booking that is not completed.");

        if (await reviews.AnyAsync(r => r.BookingId == dto.BookingId, ct))
            throw new BusinessRuleException("A review already exists for this booking.");

        var listing = await catalog.GetListingSummaryAsync(booking.ServiceListingId, ct)
            ?? throw new BusinessRuleException("Cannot determine service provider for this booking.");

        var review = new Review
        {
            Id = Guid.NewGuid(),
            Rating = dto.Rating,
            Comment = dto.Comment,
            CreatedAt = DateTime.UtcNow,
            BookingId = dto.BookingId,
            ReviewerProfileId = reviewerProfileId,
            ReviewedProfileId = listing.UserProfileId
        };

        reviews.Add(review);
        await uow.SaveChangesAsync(ct);
        return ToReviewDto(review);
    }

    public async Task<(IEnumerable<ReviewDto> Items, int TotalCount)> GetByProfileAsync(
        Guid profileId, int page, int pageSize, CancellationToken ct = default)
    {
        var query = reviews.Query().Where(r => r.ReviewedProfileId == profileId);
        var totalCount = await mat.CountAsync(query, ct);
        var items = await mat.ToListAsync(
            query.OrderByDescending(r => r.CreatedAt).Skip((page - 1) * pageSize).Take(pageSize), ct);
        return (items.Select(ToReviewDto), totalCount);
    }

    public async Task<ReviewDto> UpdateAsync(Guid reviewerProfileId, UpdateReviewDto dto, CancellationToken ct = default)
    {
        var review = await reviews.FirstOrDefaultAsync(r => r.Id == dto.Id, ct)
            ?? throw new KeyNotFoundException($"Review {dto.Id} not found.");

        if (review.ReviewerProfileId != reviewerProfileId)
            throw new BusinessRuleException("You do not own this review.");

        review.Rating = dto.Rating;
        review.Comment = dto.Comment;
        await uow.SaveChangesAsync(ct);
        return ToReviewDto(review);
    }

    public async Task DeleteAsync(Guid reviewerProfileId, Guid reviewId, CancellationToken ct = default)
    {
        var review = await reviews.FirstOrDefaultAsync(r => r.Id == reviewId, ct)
            ?? throw new KeyNotFoundException($"Review {reviewId} not found.");

        if (review.ReviewerProfileId != reviewerProfileId)
            throw new BusinessRuleException("You do not own this review.");

        reviews.Remove(review);
        await uow.SaveChangesAsync(ct);
    }

    public Task<RatingStatsDto> GetRatingStatsForProfileAsync(Guid profileId, CancellationToken ct = default)
        => ComputeRatingStatsAsync(reviews.Query().Where(r => r.ReviewedProfileId == profileId), ct);

    public Task<RatingStatsDto> GetRatingStatsForListingAsync(Guid listingId, CancellationToken ct = default)
        => ComputeRatingStatsAsync(reviews.Query().Where(r => r.Booking!.ServiceListingId == listingId), ct);

    private async Task<RatingStatsDto> ComputeRatingStatsAsync(IQueryable<Review> query, CancellationToken ct)
    {
        var count = await mat.CountAsync(query, ct);
        if (count == 0)
            return new RatingStatsDto { AverageRating = 0, ReviewCount = 0 };

        var sum = await mat.SumAsync(query, r => (decimal?)r.Rating, ct);
        return new RatingStatsDto
        {
            AverageRating = Math.Round((double)sum / count, 2),
            ReviewCount = count
        };
    }

    private static ReviewDto ToReviewDto(Review review) =>
        new()
        {
            Id = review.Id,
            Rating = review.Rating,
            Comment = review.Comment,
            CreatedAt = review.CreatedAt,
            BookingId = review.BookingId,
            ReviewerProfileId = review.ReviewerProfileId,
            ReviewedProfileId = review.ReviewedProfileId
        };
}
