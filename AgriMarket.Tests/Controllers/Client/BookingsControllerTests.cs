using AgriMarket.Modules.Bookings.Dtos.Bookings;
using AgriMarket.Modules.Bookings.Enums;
using AgriMarket.Modules.Bookings.Services;
using AgriMarket.Modules.Users.Dtos;
using AgriMarket.Modules.Users.Services;
using AgriMarket.Tests.Helpers;
using AgriMarket.Web.Areas.Client.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace AgriMarket.Tests.Controllers.Client;

public class BookingsControllerTests
{
    private readonly Mock<IBookingService> _bookingService = new();
    private readonly Mock<IUserService> _userService = new();
    private readonly Mock<IClientPaymentService> _clientPaymentService = new();
    private readonly Mock<IReviewService> _reviewService = new();

    private BookingsController CreateController(Guid userId)
    {
        var controller = new BookingsController(
            _bookingService.Object, _userService.Object,
            _clientPaymentService.Object, _reviewService.Object);
        controller.ControllerContext = ControllerContextFactory.WithAuthenticatedUser(userId);
        return controller;
    }

    [Fact]
    public async Task Index_UserNotFound_ReturnsUnauthorized()
    {
        var userId = Guid.NewGuid();
        _userService.Setup(x => x.GetProfileByUserIdAsync(userId, default))
            .ReturnsAsync((UserProfileDto?)null);

        var controller = CreateController(userId);
        var result = await controller.Index();
        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public async Task Index_WithBookings_ReturnsView()
    {
        var userId = Guid.NewGuid();
        var profileId = Guid.NewGuid();
        _userService.Setup(x => x.GetProfileByUserIdAsync(userId, default))
            .ReturnsAsync(new UserProfileDto { Id = profileId });
        _bookingService.Setup(x => x.GetByClientAsync(profileId, default))
            .ReturnsAsync(new[] { new BookingDto { Id = Guid.NewGuid(), Status = BookingStatus.Pending } });

        var controller = CreateController(userId);
        var result = await controller.Index();
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.NotNull(viewResult.Model);
    }
}
