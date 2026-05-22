using AgriMarket.Modules.Bookings.Dtos.Reviews;
using AgriMarket.Modules.Bookings.Entities;
using AgriMarket.Modules.Bookings.Enums;
using AgriMarket.Modules.Bookings.Persistence;
using AgriMarket.Modules.Bookings.Services;
using AgriMarket.Modules.Marketplace.Contracts;
using AgriMarket.Shared.Exceptions;
using AgriMarket.Shared.Persistence;
using FluentAssertions;
using Moq;
using System.Linq.Expressions;
using Xunit;

namespace AgriMarket.Tests.Services;

public class ReviewServiceTests
{
    private readonly Mock<IRepository<Review>> _reviews = new();
    private readonly Mock<IBookingRepository> _bookings = new();
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly Mock<IQueryMaterializer> _mat = new();
    private readonly Mock<ICatalogModule> _catalog = new();
    private readonly ReviewService _sut;

    private static readonly Guid ReviewerProfileId = Guid.NewGuid();
    private static readonly Guid ReviewedProfileId = Guid.NewGuid();
    private static readonly Guid BookingId = Guid.NewGuid();
    private static readonly Guid ListingId = Guid.NewGuid();

    public ReviewServiceTests()
    {
        _sut = new ReviewService(
            _reviews.Object,
            _bookings.Object,
            _uow.Object,
            _mat.Object,
            _catalog.Object);
    }

    private void SetupListingExists(Guid? userProfileId = null)
    {
        _catalog
            .Setup(c => c.GetListingSummaryAsync(ListingId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ListingSummaryDto(
                ListingId,
                "Test Service",
                50m,
                true,
                userProfileId ?? ReviewedProfileId,
                Guid.NewGuid(),
                null));
    }

    private Booking CreateCompletedBooking(Guid? clientProfileId = null)
    {
        return new Booking
        {
            Id = BookingId,
            ClientProfileId = clientProfileId ?? ReviewerProfileId,
            ServiceListingId = ListingId,
            Status = BookingStatus.ClientConfirmed,
            TotalPrice = 100,
            AreaInHectares = 2,
            CreatedAt = DateTime.UtcNow
        };
    }

    private void SetupBookingExists(Booking booking)
    {
        _bookings
            .Setup(r => r.GetByIdWithDetailsAsync(BookingId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(booking);
    }

    private static CreateReviewDto ValidDto() => new()
    {
        BookingId = BookingId,
        Rating = 5,
        Comment = "Excellent service"
    };

    [Fact]
    public async Task CreateAsync_ValidClientReview_ReturnsReviewDto()
    {
        SetupBookingExists(CreateCompletedBooking());
        SetupListingExists();

        var result = await _sut.CreateAsync(ReviewerProfileId, ValidDto());

        Assert.Equal(5, result.Rating);
        Assert.Equal("Excellent service", result.Comment);
        Assert.Equal(BookingId, result.BookingId);
        Assert.Equal(ReviewerProfileId, result.ReviewerProfileId);
        Assert.Equal(ReviewedProfileId, result.ReviewedProfileId);
        _reviews.Verify(r => r.Add(It.Is<Review>(rev =>
            rev.Rating == 5 &&
            rev.ReviewedProfileId == ReviewedProfileId)), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_ProviderCompletedStatus_Succeeds()
    {
        var booking = CreateCompletedBooking();
        booking.Status = BookingStatus.ProviderCompleted;
        SetupBookingExists(booking);
        SetupListingExists();

        var result = await _sut.CreateAsync(ReviewerProfileId, ValidDto());

        Assert.Equal(5, result.Rating);
    }

    [Fact]
    public async Task CreateAsync_BookingNotFound_ThrowsKeyNotFoundException()
    {
        _bookings
            .Setup(r => r.GetByIdWithDetailsAsync(BookingId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Booking?)null);

        var ex = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _sut.CreateAsync(ReviewerProfileId, ValidDto()));

        Assert.Equal("Booking not found.", ex.Message);
    }

    [Fact]
    public async Task CreateAsync_NonClientUser_ThrowsBusinessRuleException()
    {
        var unrelatedClientId = Guid.NewGuid();
        var booking = CreateCompletedBooking(clientProfileId: unrelatedClientId);
        SetupBookingExists(booking);

        var ex = await Assert.ThrowsAsync<BusinessRuleException>(
            () => _sut.CreateAsync(ReviewerProfileId, ValidDto()));

        Assert.Equal("Only the client can review a booking.", ex.Message);
    }

    [Theory]
    [InlineData((int)BookingStatus.Pending)]
    [InlineData((int)BookingStatus.Confirmed)]
    [InlineData((int)BookingStatus.InProgress)]
    [InlineData((int)BookingStatus.Cancelled)]
    [InlineData((int)BookingStatus.Disputed)]
    [InlineData((int)BookingStatus.AwaitingPayment)]
    public async Task CreateAsync_BookingNotCompleted_ThrowsBusinessRuleException(int statusValue)
    {
        var booking = CreateCompletedBooking();
        booking.Status = (BookingStatus)statusValue;
        SetupBookingExists(booking);

        var ex = await Assert.ThrowsAsync<BusinessRuleException>(
            () => _sut.CreateAsync(ReviewerProfileId, ValidDto()));

        Assert.Equal("Cannot review a booking that is not completed.", ex.Message);
    }

    [Fact]
    public async Task CreateAsync_ListingNotFound_ThrowsBusinessRuleException()
    {
        SetupBookingExists(CreateCompletedBooking());
        _catalog
            .Setup(c => c.GetListingSummaryAsync(ListingId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ListingSummaryDto?)null);

        var ex = await Assert.ThrowsAsync<BusinessRuleException>(
            () => _sut.CreateAsync(ReviewerProfileId, ValidDto()));

        Assert.Equal("Cannot determine service provider for this booking.", ex.Message);
    }

    [Fact]
    public async Task CreateAsync_NonClientProfileId_ThrowsBusinessRuleException()
    {
        // A provider profile (not the booking client) attempts to review.
        // The booking's ClientProfileId differs from the caller's profileId.
        var providerProfileId = ReviewedProfileId;
        var booking = CreateCompletedBooking(clientProfileId: Guid.NewGuid());
        SetupBookingExists(booking);

        var ex = await Assert.ThrowsAsync<BusinessRuleException>(
            () => _sut.CreateAsync(providerProfileId, ValidDto()));

        Assert.Equal("Only the client can review a booking.", ex.Message);
    }

    [Fact]
    public async Task GetByIdAsync_ReviewExists_ReturnsDto()
    {
        var reviewId = Guid.NewGuid();
        _reviews
            .Setup(r => r.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<Review, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Review
            {
                Id = reviewId,
                Rating = 4,
                Comment = "Good",
                CreatedAt = DateTime.UtcNow,
                BookingId = BookingId,
                ReviewerProfileId = ReviewerProfileId
            });

        var result = await _sut.GetByIdAsync(reviewId);

        Assert.NotNull(result);
        Assert.Equal(reviewId, result!.Id);
        Assert.Equal(4, result.Rating);
        Assert.Equal("Good", result.Comment);
    }

    [Fact]
    public async Task GetByIdAsync_ReviewNotFound_ReturnsNull()
    {
        _reviews
            .Setup(r => r.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<Review, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Review?)null);

        var result = await _sut.GetByIdAsync(Guid.NewGuid());

        Assert.Null(result);
    }

    [Fact]
    public async Task GetByBookingAsync_ReviewExists_ReturnsDto()
    {
        var review = new Review
        {
            Id = Guid.NewGuid(),
            Rating = 5,
            CreatedAt = DateTime.UtcNow,
            BookingId = BookingId,
            ReviewerProfileId = ReviewerProfileId,
            ReviewedProfileId = ReviewedProfileId
        };
        _reviews
            .Setup(r => r.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<Review, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(review);

        var result = await _sut.GetByBookingAsync(BookingId);

        Assert.NotNull(result);
        Assert.Equal(review.Id, result!.Id);
        Assert.Equal(5, result.Rating);
    }

    [Fact]
    public async Task GetByBookingAsync_NoReview_ReturnsNull()
    {
        _reviews
            .Setup(r => r.FirstOrDefaultAsync(
                It.IsAny<Expression<Func<Review, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Review?)null);

        var result = await _sut.GetByBookingAsync(BookingId);

        Assert.Null(result);
    }

    [Fact]
    public async Task CreateAsync_MinimumRating_Succeeds()
    {
        SetupBookingExists(CreateCompletedBooking());
        SetupListingExists();
        var dto = new CreateReviewDto { BookingId = BookingId, Rating = 1 };

        var result = await _sut.CreateAsync(ReviewerProfileId, dto);

        Assert.Equal(1, result.Rating);
        Assert.Null(result.Comment);
    }

    [Fact]
    public async Task CreateAsync_NullComment_Succeeds()
    {
        SetupBookingExists(CreateCompletedBooking());
        SetupListingExists();
        var dto = new CreateReviewDto { BookingId = BookingId, Rating = 3, Comment = null };

        var result = await _sut.CreateAsync(ReviewerProfileId, dto);

        Assert.Null(result.Comment);
    }

    [Fact]
    public async Task CreateAsync_DuplicateReview_ThrowsBusinessRuleException()
    {
        SetupBookingExists(CreateCompletedBooking());
        _reviews.Setup(r => r.AnyAsync(
            It.IsAny<Expression<Func<Review, bool>>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var ex = await Assert.ThrowsAsync<BusinessRuleException>(
            () => _sut.CreateAsync(ReviewerProfileId, ValidDto()));

        Assert.Equal("A review already exists for this booking.", ex.Message);
    }

    [Fact]
    public async Task UpdateAsync_OwnerUpdatesReview_ReturnsUpdatedDto()
    {
        var reviewId = Guid.NewGuid();
        _reviews.Setup(r => r.FirstOrDefaultAsync(
            It.IsAny<Expression<Func<Review, bool>>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Review
            {
                Id = reviewId,
                Rating = 3,
                Comment = "Old",
                BookingId = BookingId,
                ReviewerProfileId = ReviewerProfileId,
                ReviewedProfileId = ReviewedProfileId,
                CreatedAt = DateTime.UtcNow
            });

        var result = await _sut.UpdateAsync(ReviewerProfileId, new UpdateReviewDto { Id = reviewId, Rating = 5, Comment = "Updated" });

        Assert.Equal(5, result.Rating);
        Assert.Equal("Updated", result.Comment);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ReviewNotFound_ThrowsKeyNotFoundException()
    {
        _reviews.Setup(r => r.FirstOrDefaultAsync(
            It.IsAny<Expression<Func<Review, bool>>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync((Review?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _sut.UpdateAsync(ReviewerProfileId, new UpdateReviewDto { Id = Guid.NewGuid(), Rating = 4 }));
    }

    [Fact]
    public async Task UpdateAsync_NotOwner_ThrowsBusinessRuleException()
    {
        var reviewId = Guid.NewGuid();
        _reviews.Setup(r => r.FirstOrDefaultAsync(
            It.IsAny<Expression<Func<Review, bool>>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Review
            {
                Id = reviewId,
                Rating = 3,
                ReviewerProfileId = Guid.NewGuid(),
                BookingId = BookingId,
                ReviewedProfileId = ReviewedProfileId,
                CreatedAt = DateTime.UtcNow
            });

        var ex = await Assert.ThrowsAsync<BusinessRuleException>(
            () => _sut.UpdateAsync(ReviewerProfileId, new UpdateReviewDto { Id = reviewId, Rating = 4 }));

        Assert.Equal("You do not own this review.", ex.Message);
    }

    [Fact]
    public async Task DeleteAsync_OwnerDeletesReview_Succeeds()
    {
        var reviewId = Guid.NewGuid();
        var review = new Review
        {
            Id = reviewId,
            Rating = 3,
            ReviewerProfileId = ReviewerProfileId,
            BookingId = BookingId,
            ReviewedProfileId = ReviewedProfileId,
            CreatedAt = DateTime.UtcNow
        };
        _reviews.Setup(r => r.FirstOrDefaultAsync(
            It.IsAny<Expression<Func<Review, bool>>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(review);

        await _sut.DeleteAsync(ReviewerProfileId, reviewId);

        _reviews.Verify(r => r.Remove(review), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ReviewNotFound_ThrowsKeyNotFoundException()
    {
        _reviews.Setup(r => r.FirstOrDefaultAsync(
            It.IsAny<Expression<Func<Review, bool>>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync((Review?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => _sut.DeleteAsync(ReviewerProfileId, Guid.NewGuid()));
    }

    [Fact]
    public async Task DeleteAsync_NotOwner_ThrowsBusinessRuleException()
    {
        var reviewId = Guid.NewGuid();
        _reviews.Setup(r => r.FirstOrDefaultAsync(
            It.IsAny<Expression<Func<Review, bool>>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Review
            {
                Id = reviewId,
                Rating = 3,
                ReviewerProfileId = Guid.NewGuid(),
                BookingId = BookingId,
                ReviewedProfileId = ReviewedProfileId,
                CreatedAt = DateTime.UtcNow
            });

        var ex = await Assert.ThrowsAsync<BusinessRuleException>(
            () => _sut.DeleteAsync(ReviewerProfileId, reviewId));

        Assert.Equal("You do not own this review.", ex.Message);
    }

    [Fact]
    public async Task GetByProfileAsync_ReturnsFilteredPaginatedResults()
    {
        var reviews = new List<Review>
        {
            new() { Id = Guid.NewGuid(), Rating = 4, CreatedAt = DateTime.UtcNow,
                BookingId = BookingId, ReviewerProfileId = ReviewerProfileId, ReviewedProfileId = ReviewedProfileId }
        };
        _mat.Setup(m => m.CountAsync(It.IsAny<IQueryable<Review>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
        _mat.Setup(m => m.ToListAsync(It.IsAny<IQueryable<Review>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(reviews);

        var (items, totalCount) = await _sut.GetByProfileAsync(ReviewedProfileId, 1, 10);
        var list = items.ToList();

        Assert.Equal(1, totalCount);
        Assert.Single(list);
        Assert.Equal(4, list[0].Rating);
    }

    [Fact]
    public async Task GetRatingStatsForProfileAsync_WithReviews_ReturnsStats()
    {
        _mat.Setup(m => m.CountAsync(It.IsAny<IQueryable<Review>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(3);
        _mat.Setup(m => m.SumAsync(
            It.IsAny<IQueryable<Review>>(),
            It.IsAny<Expression<Func<Review, decimal?>>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(12m);

        var stats = await _sut.GetRatingStatsForProfileAsync(ReviewedProfileId);

        Assert.Equal(4.0, stats.AverageRating);
        Assert.Equal(3, stats.ReviewCount);
    }

    [Fact]
    public async Task GetRatingStatsForProfileAsync_NoReviews_ReturnsZero()
    {
        _mat.Setup(m => m.CountAsync(It.IsAny<IQueryable<Review>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        var stats = await _sut.GetRatingStatsForProfileAsync(ReviewedProfileId);

        Assert.Equal(0, stats.AverageRating);
        Assert.Equal(0, stats.ReviewCount);
    }

    [Fact]
    public async Task GetRatingStatsForListingAsync_WithReviews_ReturnsStats()
    {
        _mat.Setup(m => m.CountAsync(It.IsAny<IQueryable<Review>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(2);
        _mat.Setup(m => m.SumAsync(
            It.IsAny<IQueryable<Review>>(),
            It.IsAny<Expression<Func<Review, decimal?>>>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(9m);

        var stats = await _sut.GetRatingStatsForListingAsync(ListingId);

        Assert.Equal(4.5, stats.AverageRating);
        Assert.Equal(2, stats.ReviewCount);
    }

    [Fact]
    public async Task GetRatingStatsForListingAsync_NoReviews_ReturnsZero()
    {
        _mat.Setup(m => m.CountAsync(It.IsAny<IQueryable<Review>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        var stats = await _sut.GetRatingStatsForListingAsync(ListingId);

        Assert.Equal(0, stats.AverageRating);
        Assert.Equal(0, stats.ReviewCount);
    }
}
