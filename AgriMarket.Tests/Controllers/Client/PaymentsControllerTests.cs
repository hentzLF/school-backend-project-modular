using AgriMarket.Modules.Bookings.Dtos.Payments;
using AgriMarket.Modules.Bookings.Entities;
using AgriMarket.Modules.Bookings.Enums;
using AgriMarket.Modules.Bookings.Services;
using AgriMarket.Modules.Users.Dtos;
using AgriMarket.Modules.Users.Services;
using AgriMarket.Tests.Helpers;
using AgriMarket.Web.Areas.Client.Controllers;
using AgriMarket.Web.Areas.Client.ViewModels.Payments;
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

    [Fact]
    public async Task Receipt_WithValidPayment_ReturnsView()
    {
        var userId = Guid.NewGuid();
        var profileId = Guid.NewGuid();
        var bookingId = Guid.NewGuid();
        var paymentId = Guid.NewGuid();

        _userService.Setup(x => x.GetProfileByUserIdAsync(userId, default))
            .ReturnsAsync(new UserProfileDto { Id = profileId });

        var payment = new Payment
        {
            Id = paymentId,
            BookingId = bookingId,
            Amount = 100m,
            PlatformFee = 5m,
            Method = PaymentMethod.Card,
            Status = PaymentStatus.Held,
            CreatedAt = DateTime.UtcNow
        };
        _paymentService.Setup(x => x.GetByIdAsync(paymentId))
            .ReturnsAsync(payment);

        _clientPaymentService.Setup(x => x.GetHistoryAsync(profileId, default))
            .ReturnsAsync(
            [
                new PaymentHistoryItemDto(paymentId, bookingId, "Test", 100m, 5m, "Card", "Held", DateTime.UtcNow, null)
            ]);

        var controller = CreateController(userId);
        var result = await controller.Receipt(paymentId);

        var viewResult = Assert.IsType<ViewResult>(result);
        var vm = Assert.IsType<ReceiptViewModel>(viewResult.Model);
        Assert.Equal(paymentId, vm.PaymentId);
        Assert.Equal(bookingId, vm.BookingId);
    }

    [Fact]
    public async Task Receipt_WithNonExistentPayment_ReturnsNotFound()
    {
        var userId = Guid.NewGuid();
        var profileId = Guid.NewGuid();

        _userService.Setup(x => x.GetProfileByUserIdAsync(userId, default))
            .ReturnsAsync(new UserProfileDto { Id = profileId });
        _paymentService.Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Payment?)null);

        var controller = CreateController(userId);
        var result = await controller.Receipt(Guid.NewGuid());

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Index_WithPayments_ReturnsViewWithHistory()
    {
        var userId = Guid.NewGuid();
        var profileId = Guid.NewGuid();

        _userService.Setup(x => x.GetProfileByUserIdAsync(userId, default))
            .ReturnsAsync(new UserProfileDto { Id = profileId });
        _clientPaymentService.Setup(x => x.GetHistoryAsync(profileId, default))
            .ReturnsAsync(
            [
                new PaymentHistoryItemDto(Guid.NewGuid(), Guid.NewGuid(), "Service", 200m, 10m, "Card", "Released", DateTime.UtcNow, DateTime.UtcNow)
            ]);

        var controller = CreateController(userId);
        var result = await controller.Index();

        var viewResult = Assert.IsType<ViewResult>(result);
        var vm = Assert.IsType<PaymentHistoryViewModel>(viewResult.Model);
        Assert.Single(vm.Payments);
    }
}
