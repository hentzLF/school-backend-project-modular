using AgriMarket.Modules.Bookings.Services;
using AgriMarket.Modules.Marketplace.Dtos.Listings;
using AgriMarket.Modules.Marketplace.Services;
using AgriMarket.Modules.Users.Dtos;
using AgriMarket.Modules.Users.Services;
using AgriMarket.Tests.Helpers;
using AgriMarket.Web.Areas.Client.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace AgriMarket.Tests.Controllers.Client;

public class MyListingsControllerTests
{
    private readonly Mock<IListingService> _listingService = new();
    private readonly Mock<ICategoryService> _categoryService = new();
    private readonly Mock<IBookingService> _bookingService = new();
    private readonly Mock<IUserService> _userService = new();
    private readonly Mock<IEquipmentService> _equipmentService = new();

    private MyListingsController CreateController(Guid userId)
    {
        var controller = new MyListingsController(
            _listingService.Object, _categoryService.Object,
            _bookingService.Object, _userService.Object, _equipmentService.Object);
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
    public async Task Index_WithListings_ReturnsView()
    {
        var userId = Guid.NewGuid();
        var profileId = Guid.NewGuid();
        _userService.Setup(x => x.GetProfileByUserIdAsync(userId, default))
            .ReturnsAsync(new UserProfileDto { Id = profileId });
        _listingService.Setup(x => x.GetByProviderAsync(profileId, default))
            .ReturnsAsync(new List<ListingSummaryDto>());

        var controller = CreateController(userId);
        var result = await controller.Index();
        Assert.IsType<ViewResult>(result);
    }
}
