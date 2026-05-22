using AgriMarket.Modules.Bookings.Dtos.Reviews;
using AgriMarket.Web.Areas.Client.ViewModels.Reviews;
using AgriMarket.Web.Mappers;
using Xunit;

namespace AgriMarket.Tests.Mappers;

public class ReviewViewModelMapperTests
{
    [Fact]
    public void ToCreateDto_MapsAllProperties()
    {
        // Arrange
        var vm = new CreateReviewViewModel
        {
            BookingId = Guid.NewGuid(),
            Rating = 4,
            Comment = "Great service"
        };

        // Act
        var dto = vm.ToCreateDto();

        // Assert
        Assert.Equal(vm.BookingId, dto.BookingId);
        Assert.Equal(vm.Rating, dto.Rating);
        Assert.Equal(vm.Comment, dto.Comment);
    }

    [Fact]
    public void ToCreateDto_WithNullComment_MapsCorrectly()
    {
        // Arrange
        var vm = new CreateReviewViewModel
        {
            BookingId = Guid.NewGuid(),
            Rating = 5,
            Comment = null
        };

        // Act
        var dto = vm.ToCreateDto();

        // Assert
        Assert.Null(dto.Comment);
    }

    [Fact]
    public void ToUpdateDto_MapsAllProperties()
    {
        // Arrange
        var vm = new EditReviewViewModel
        {
            ReviewId = Guid.NewGuid(),
            BookingId = Guid.NewGuid(),
            Rating = 3,
            Comment = "Updated comment"
        };

        // Act
        var dto = vm.ToUpdateDto();

        // Assert
        Assert.Equal(vm.ReviewId, dto.Id);
        Assert.Equal(vm.Rating, dto.Rating);
        Assert.Equal(vm.Comment, dto.Comment);
    }

    [Fact]
    public void ToViewModel_MapsAllProperties()
    {
        // Arrange
        var dto = new ReviewDto
        {
            Id = Guid.NewGuid(),
            BookingId = Guid.NewGuid(),
            Rating = 5,
            Comment = "Excellent",
            CreatedAt = DateTime.UtcNow,
            ReviewerProfileId = Guid.NewGuid(),
            ReviewedProfileId = Guid.NewGuid()
        };

        // Act
        var vm = dto.ToViewModel("John Doe");

        // Assert
        Assert.Equal(dto.Id, vm.Id);
        Assert.Equal(dto.BookingId, vm.BookingId);
        Assert.Equal(dto.Rating, vm.Rating);
        Assert.Equal(dto.Comment, vm.Comment);
        Assert.Equal("John Doe", vm.ReviewerName);
        Assert.Equal(dto.CreatedAt, vm.CreatedAt);
    }

    [Fact]
    public void ToEditViewModel_MapsAllProperties()
    {
        // Arrange
        var dto = new ReviewDto
        {
            Id = Guid.NewGuid(),
            BookingId = Guid.NewGuid(),
            Rating = 2,
            Comment = "Could be better"
        };

        // Act
        var vm = dto.ToEditViewModel("Test Booking");

        // Assert
        Assert.Equal(dto.Id, vm.ReviewId);
        Assert.Equal(dto.BookingId, vm.BookingId);
        Assert.Equal(dto.Rating, vm.Rating);
        Assert.Equal(dto.Comment, vm.Comment);
        Assert.Equal("Test Booking", vm.BookingTitle);
    }

    [Fact]
    public void ToDeleteViewModel_MapsAllProperties()
    {
        // Arrange
        var dto = new ReviewDto
        {
            Id = Guid.NewGuid(),
            BookingId = Guid.NewGuid(),
            Rating = 1,
            Comment = "Bad experience",
            CreatedAt = DateTime.UtcNow
        };

        // Act
        var vm = dto.ToDeleteViewModel("Reviewer");

        // Assert
        Assert.Equal(dto.Id, vm.ReviewId);
        Assert.Equal(dto.BookingId, vm.BookingId);
        Assert.Equal(dto.Rating, vm.Rating);
        Assert.Equal(dto.Comment, vm.Comment);
        Assert.Equal("Reviewer", vm.ReviewerName);
        Assert.Equal(dto.CreatedAt, vm.CreatedAt);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    public void ToViewModel_BoundaryRatings_MapsCorrectly(int rating)
    {
        // Arrange
        var dto = new ReviewDto
        {
            Id = Guid.NewGuid(),
            BookingId = Guid.NewGuid(),
            Rating = rating,
            CreatedAt = DateTime.UtcNow
        };

        // Act
        var vm = dto.ToViewModel();

        // Assert
        Assert.Equal(rating, vm.Rating);
    }

    [Fact]
    public void ToRatingStatsViewModel_MapsAllProperties()
    {
        // Arrange
        var dto = new RatingStatsDto
        {
            AverageRating = 4.2,
            ReviewCount = 15
        };

        // Act
        var vm = dto.ToRatingStatsViewModel();

        // Assert
        Assert.Equal(4.2, vm.AverageRating);
        Assert.Equal(15, vm.ReviewCount);
    }

    [Fact]
    public void ToReviewListViewModel_MapsAllProperties()
    {
        // Arrange
        var reviews = new[]
        {
            new ReviewDto { Id = Guid.NewGuid(), BookingId = Guid.NewGuid(), Rating = 4, CreatedAt = DateTime.UtcNow },
            new ReviewDto { Id = Guid.NewGuid(), BookingId = Guid.NewGuid(), Rating = 5, CreatedAt = DateTime.UtcNow }
        };
        var stats = new RatingStatsDto { AverageRating = 4.5, ReviewCount = 2 };
        var profileId = Guid.NewGuid();

        // Act
        var vm = ReviewViewModelMapper.ToReviewListViewModel(
            reviews, stats, profileId, "Provider Name", 1, 3);

        // Assert
        Assert.Equal(2, vm.Reviews.Count());
        Assert.Equal(profileId, vm.ProfileId);
        Assert.Equal("Provider Name", vm.ProviderName);
        Assert.Equal(4.5, vm.AverageRating);
        Assert.Equal(2, vm.ReviewCount);
        Assert.Equal(1, vm.CurrentPage);
        Assert.Equal(3, vm.TotalPages);
    }
}
