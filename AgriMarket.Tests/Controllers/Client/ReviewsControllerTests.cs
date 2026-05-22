using AgriMarket.Modules.Bookings.Dtos.Reviews;
using AgriMarket.Modules.Bookings.Services;
using AgriMarket.Modules.Users.Dtos;
using AgriMarket.Modules.Users.Services;
using AgriMarket.Tests.Helpers;
using AgriMarket.Web.Areas.Client.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace AgriMarket.Tests.Controllers.Client;

public class ReviewsControllerTests
{
    private readonly Mock<IReviewService> _reviewService = new();
    private readonly Mock<IBookingService> _bookingService = new();
    private readonly Mock<IUserService> _userService = new();

    private ReviewsController CreateController(Guid userId)
    {
        var controller = new ReviewsController(
            _reviewService.Object, _bookingService.Object, _userService.Object);
        controller.ControllerContext = ControllerContextFactory.WithAuthenticatedUser(userId);
        return controller;
    }

    [Fact]
    public async Task ForProvider_UserNotFound_ReturnsNotFound()
    {
        var userId = Guid.NewGuid();
        var profileId = Guid.NewGuid();
        _userService.Setup(x => x.GetProfileByIdAsync(profileId, null, false, default))
            .ReturnsAsync((UserProfileDto?)null);

        var controller = CreateController(userId);
        var result = await controller.ForProvider(profileId);
        Assert.IsType<NotFoundResult>(result);
    }
}
