using AgriMarket.Modules.Bookings.Dtos.Bookings;
using AgriMarket.Modules.Bookings.Dtos.Reviews;
using AgriMarket.Modules.Bookings.Enums;
using AgriMarket.Modules.Bookings.Services;
using AgriMarket.Modules.Users.Dtos;
using AgriMarket.Modules.Users.Services;
using AgriMarket.Shared.Exceptions;
using AgriMarket.Tests.Helpers;
using AgriMarket.Web.Areas.Client.Controllers;
using AgriMarket.Web.Areas.Client.ViewModels.Reviews;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace AgriMarket.Tests.Controllers.Client;

public class ReviewsControllerTests
{
    private readonly Mock<IReviewService> _reviewService = new();
    private readonly Mock<IBookingService> _bookingService = new();
    private readonly Mock<IUserService> _userService = new();

    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly Guid ProfileId = Guid.NewGuid();
    private static readonly Guid OtherProfileId = Guid.NewGuid();

    private ReviewsController CreateController(Guid userId)
    {
        var controller = new ReviewsController(
            _reviewService.Object, _bookingService.Object, _userService.Object);
        controller.ControllerContext = ControllerContextFactory.WithAuthenticatedUser(userId);
        return controller;
    }

    // --- Create GET ---

    [Fact]
    public async Task CreateGet_BookingNotFound_ReturnsNotFound()
    {
        _bookingService.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), default))
            .ReturnsAsync((BookingDto?)null);

        var controller = CreateController(UserId);
        var result = await controller.Create(Guid.NewGuid());
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task CreateGet_WithValidBooking_ReturnsView()
    {
        var bookingId = Guid.NewGuid();
        _bookingService.Setup(x => x.GetByIdAsync(bookingId, default))
            .ReturnsAsync(new BookingDto { Id = bookingId, ListingTitle = "Test Listing", Status = BookingStatus.ClientConfirmed });

        var controller = CreateController(UserId);
        var result = await controller.Create(bookingId);

        var viewResult = Assert.IsType<ViewResult>(result);
        var vm = Assert.IsType<CreateReviewViewModel>(viewResult.Model);
        Assert.Equal(bookingId, vm.BookingId);
    }

    // --- Create POST ---

    [Fact]
    public async Task CreatePost_InvalidModel_ReloadsView()
    {
        var bookingId = Guid.NewGuid();
        _bookingService.Setup(x => x.GetByIdAsync(bookingId, default))
            .ReturnsAsync(new BookingDto { Id = bookingId, ListingTitle = "Test", Status = BookingStatus.ClientConfirmed });

        var controller = CreateController(UserId);
        controller.ModelState.AddModelError("Rating", "Required");

        var model = new CreateReviewViewModel { BookingId = bookingId };
        var result = await controller.Create(model);
        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public async Task CreatePost_WithValidReview_RedirectsToBookingDetails()
    {
        var bookingId = Guid.NewGuid();
        _reviewService.Setup(x => x.CreateAsync(UserId, It.IsAny<CreateReviewDto>(), default))
            .ReturnsAsync(new ReviewDto { Id = Guid.NewGuid(), BookingId = bookingId, Rating = 4 });

        var controller = CreateController(UserId);
        var model = new CreateReviewViewModel
        {
            BookingId = bookingId,
            Rating = 4,
            Comment = "Great service!",
            BookingTitle = "Test"
        };

        var result = await controller.Create(model);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Details", redirect.ActionName);
        Assert.Equal("Bookings", redirect.ControllerName);
    }

    [Fact]
    public async Task CreatePost_WithInvalidBookingState_RedirectsWithError()
    {
        var bookingId = Guid.NewGuid();
        _reviewService
            .Setup(x => x.CreateAsync(UserId, It.IsAny<CreateReviewDto>(), default))
            .ThrowsAsync(new BusinessRuleException("Booking is not in a reviewable state"));

        var controller = CreateController(UserId);
        var tempDataMock = new Mock<Microsoft.AspNetCore.Mvc.ViewFeatures.ITempDataDictionary>();
        controller.TempData = tempDataMock.Object;

        var model = new CreateReviewViewModel
        {
            BookingId = bookingId,
            Rating = 4,
            Comment = "Should fail",
            BookingTitle = "Test"
        };

        var result = await controller.Create(model);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Details", redirect.ActionName);
        Assert.Equal("Bookings", redirect.ControllerName);
    }

    // --- Edit GET ---

    [Fact]
    public async Task EditGet_WithNonExistentReview_ReturnsNotFound()
    {
        _reviewService.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), default))
            .ReturnsAsync((ReviewDto?)null);

        var controller = CreateController(UserId);
        var result = await controller.Edit(Guid.NewGuid());
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task EditGet_WithExistingReview_ReturnsView()
    {
        var reviewId = Guid.NewGuid();
        var bookingId = Guid.NewGuid();
        _reviewService.Setup(x => x.GetByIdAsync(reviewId, default))
            .ReturnsAsync(new ReviewDto { Id = reviewId, BookingId = bookingId, Rating = 4, Comment = "Good" });
        _bookingService.Setup(x => x.GetByIdAsync(bookingId, default))
            .ReturnsAsync(new BookingDto { Id = bookingId, ListingTitle = "Test Listing" });

        var controller = CreateController(UserId);
        var result = await controller.Edit(reviewId);

        var viewResult = Assert.IsType<ViewResult>(result);
        var vm = Assert.IsType<EditReviewViewModel>(viewResult.Model);
        Assert.Equal(reviewId, vm.ReviewId);
    }

    // --- Delete GET ---

    [Fact]
    public async Task DeleteGet_WithNonExistentReview_ReturnsNotFound()
    {
        _reviewService.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), default))
            .ReturnsAsync((ReviewDto?)null);

        var controller = CreateController(UserId);
        var result = await controller.Delete(Guid.NewGuid());
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task DeleteGet_WithExistingReview_ReturnsView()
    {
        var reviewId = Guid.NewGuid();
        var bookingId = Guid.NewGuid();
        _reviewService.Setup(x => x.GetByIdAsync(reviewId, default))
            .ReturnsAsync(new ReviewDto { Id = reviewId, BookingId = bookingId, Rating = 4, Comment = "Good service" });

        var controller = CreateController(UserId);
        var result = await controller.Delete(reviewId);

        var viewResult = Assert.IsType<ViewResult>(result);
        var vm = Assert.IsType<DeleteReviewViewModel>(viewResult.Model);
        Assert.Equal(reviewId, vm.ReviewId);
    }

    // --- DeleteConfirmed ---

    [Fact]
    public async Task DeletePost_ReviewNotFound_ReturnsNotFound()
    {
        _reviewService.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), default))
            .ReturnsAsync((ReviewDto?)null);

        var controller = CreateController(UserId);
        var result = await controller.DeleteConfirmed(Guid.NewGuid());
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task DeletePost_WithExistingReview_DeletesAndRedirects()
    {
        var reviewId = Guid.NewGuid();
        var bookingId = Guid.NewGuid();
        _reviewService.Setup(x => x.GetByIdAsync(reviewId, default))
            .ReturnsAsync(new ReviewDto { Id = reviewId, BookingId = bookingId, Rating = 4 });
        _reviewService.Setup(x => x.DeleteAsync(UserId, reviewId, default))
            .Returns(Task.CompletedTask);

        var controller = CreateController(UserId);
        var result = await controller.DeleteConfirmed(reviewId);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Details", redirect.ActionName);
        Assert.Equal("Bookings", redirect.ControllerName);
        _reviewService.Verify(x => x.DeleteAsync(UserId, reviewId, default), Times.Once);
    }

    // --- ForProvider ---

    [Fact]
    public async Task ForProvider_UserNotFound_ReturnsNotFound()
    {
        _userService.Setup(x => x.GetProfileByIdAsync(OtherProfileId, null, false, default))
            .ReturnsAsync((UserProfileDto?)null);

        var controller = CreateController(UserId);
        var result = await controller.ForProvider(OtherProfileId);
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task ForProvider_WithReviews_ReturnsViewWithList()
    {
        _userService.Setup(x => x.GetProfileByIdAsync(OtherProfileId, null, false, default))
            .ReturnsAsync(new UserProfileDto { Id = OtherProfileId, FirstName = "John", LastName = "Doe" });
        _reviewService.Setup(x => x.GetByProfileAsync(OtherProfileId, 1, 10, default))
            .ReturnsAsync((new List<ReviewDto>
            {
                new() { Id = Guid.NewGuid(), Rating = 5, Comment = "Great!" }
            }, 1));
        _reviewService.Setup(x => x.GetRatingStatsForProfileAsync(OtherProfileId, default))
            .ReturnsAsync(new RatingStatsDto { AverageRating = 5.0, ReviewCount = 1 });

        var controller = CreateController(UserId);
        var result = await controller.ForProvider(OtherProfileId);

        var viewResult = Assert.IsType<ViewResult>(result);
        var vm = Assert.IsType<ReviewListViewModel>(viewResult.Model);
        Assert.Single(vm.Reviews);
    }

    [Fact]
    public async Task ForProvider_WithNoReviews_ReturnsEmptyView()
    {
        _userService.Setup(x => x.GetProfileByIdAsync(OtherProfileId, null, false, default))
            .ReturnsAsync(new UserProfileDto { Id = OtherProfileId, FirstName = "John", LastName = "Doe" });
        _reviewService.Setup(x => x.GetByProfileAsync(OtherProfileId, 1, 10, default))
            .ReturnsAsync((new List<ReviewDto>(), 0));
        _reviewService.Setup(x => x.GetRatingStatsForProfileAsync(OtherProfileId, default))
            .ReturnsAsync(new RatingStatsDto { AverageRating = 0.0, ReviewCount = 0 });

        var controller = CreateController(UserId);
        var result = await controller.ForProvider(OtherProfileId);

        var viewResult = Assert.IsType<ViewResult>(result);
        var vm = Assert.IsType<ReviewListViewModel>(viewResult.Model);
        Assert.Empty(vm.Reviews);
    }
}
