using System.Reflection;
using AgriMarket.Modules.Bookings.Services;
using AgriMarket.Modules.Marketplace.Dtos.Listings;
using AgriMarket.Modules.Marketplace.Services;
using AgriMarket.Modules.Users.Dtos;
using AgriMarket.Modules.Users.Services;
using AgriMarket.Tests.Helpers;
using AgriMarket.Web.Areas.Client.Controllers;
using AgriMarket.Web.Areas.Client.ViewModels.MyListings;
using Microsoft.AspNetCore.Authorization;
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
    public void Controller_HasClientOnlyPolicy()
    {
        var attr = typeof(MyListingsController).GetCustomAttribute<AuthorizeAttribute>();
        Assert.NotNull(attr);
        Assert.Equal("ClientOnly", attr.Policy);
    }

    [Fact]
    public async Task Index_UserNotFound_ReturnsNotFound()
    {
        var userId = Guid.NewGuid();
        _userService.Setup(x => x.GetProfileByUserIdAsync(userId, default))
            .ReturnsAsync((UserProfileDto?)null);

        var controller = CreateController(userId);
        var result = await controller.Index();
        Assert.IsType<NotFoundResult>(result);
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

    [Fact]
    public async Task Availabilities_OtherProviderListing_ReturnsNotFound()
    {
        // IDOR: a provider cannot view availability slots for another provider's listing
        var userId = Guid.NewGuid();
        var ownProfileId = Guid.NewGuid();
        var listingId = Guid.NewGuid();

        // Profile belongs to userId, but listing belongs to a different profile
        _userService.Setup(x => x.GetProfileByUserIdAsync(userId, default))
            .ReturnsAsync(new UserProfileDto { Id = ownProfileId });
        _listingService.Setup(x => x.GetByIdAsync(listingId, default))
            .ReturnsAsync(new ListingDto { Id = listingId, UserProfileId = Guid.NewGuid() });

        var controller = CreateController(userId);
        var result = await controller.Availabilities(listingId);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task AddAvailability_InvalidDates_AddsModelError()
    {
        // Start >= End must be rejected with a validation error
        var userId = Guid.NewGuid();
        var profileId = Guid.NewGuid();
        var listingId = Guid.NewGuid();

        _userService.Setup(x => x.GetProfileByUserIdAsync(userId, default))
            .ReturnsAsync(new UserProfileDto { Id = profileId });
        _listingService.Setup(x => x.GetByIdAsync(listingId, default))
            .ReturnsAsync(new ListingDto { Id = listingId, UserProfileId = profileId });

        var controller = CreateController(userId);

        var model = new ManageAvailabilitiesViewModel
        {
            AddStartTime = DateTime.UtcNow.AddDays(1),
            AddEndTime = DateTime.UtcNow // Start > End
        };

        var result = await controller.AddAvailability(listingId, model);

        Assert.False(controller.ModelState.IsValid);
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("Availabilities", viewResult.ViewName);
    }
}
