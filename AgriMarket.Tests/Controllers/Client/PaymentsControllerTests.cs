using AgriMarket.Modules.Bookings.Services;
using AgriMarket.Modules.Users.Dtos;
using AgriMarket.Modules.Users.Services;
using AgriMarket.Tests.Helpers;
using AgriMarket.Web.Areas.Client.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace AgriMarket.Tests.Controllers.Client;

public class PaymentsControllerTests
{
    private readonly Mock<IPaymentService> _paymentService = new();
    private readonly Mock<IClientPaymentService> _clientPaymentService = new();
    private readonly Mock<IUserService> _userService = new();

    private PaymentsController CreateController(Guid userId)
    {
        var controller = new PaymentsController(
            _paymentService.Object, _clientPaymentService.Object, _userService.Object);
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
}
