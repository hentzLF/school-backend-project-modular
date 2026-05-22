using AgriMarket.Modules.Bookings.Dtos.Bookings;
using AgriMarket.Modules.Bookings.Dtos.Payments;
using AgriMarket.Modules.Bookings.Dtos.Reviews;
using AgriMarket.Modules.Bookings.Enums;
using AgriMarket.Modules.Bookings.Services;
using AgriMarket.Modules.Users.Dtos;
using AgriMarket.Modules.Users.Services;
using AgriMarket.Shared.Exceptions;
using AgriMarket.Tests.Helpers;
using AgriMarket.Web.Areas.Client.Controllers;
using AgriMarket.Web.Areas.Client.ViewModels.Bookings;
using AgriMarket.Web.Areas.Client.ViewModels.Payments;
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

    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly Guid ProfileId = Guid.NewGuid();
    private static readonly Guid OtherProfileId = Guid.NewGuid();

    private BookingsController CreateController(Guid userId)
    {
        var controller = new BookingsController(
            _bookingService.Object, _userService.Object,
            _clientPaymentService.Object, _reviewService.Object);
        controller.ControllerContext = ControllerContextFactory.WithAuthenticatedUser(userId);
        return controller;
    }

    private void SetupProfile(Guid userId, Guid profileId)
    {
        _userService
            .Setup(x => x.GetProfileByUserIdAsync(userId, default))
            .ReturnsAsync(new UserProfileDto { Id = profileId });
    }

    // --- Index ---

    [Fact]
    public async Task Index_UserNotFound_ReturnsUnauthorized()
    {
        _userService.Setup(x => x.GetProfileByUserIdAsync(UserId, default))
            .ReturnsAsync((UserProfileDto?)null);

        var controller = CreateController(UserId);
        var result = await controller.Index();
        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public async Task Index_WithBookings_ReturnsView()
    {
        SetupProfile(UserId, ProfileId);
        _bookingService.Setup(x => x.GetByClientAsync(ProfileId, default))
            .ReturnsAsync(new[] { new BookingDto { Id = Guid.NewGuid(), Status = BookingStatus.Pending } });

        var controller = CreateController(UserId);
        var result = await controller.Index();
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.NotNull(viewResult.Model);
    }

    // --- Details ---

    [Fact]
    public async Task Details_UserNotFound_ReturnsUnauthorized()
    {
        _userService.Setup(x => x.GetProfileByUserIdAsync(UserId, default))
            .ReturnsAsync((UserProfileDto?)null);

        var controller = CreateController(UserId);
        var result = await controller.Details(Guid.NewGuid());
        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public async Task Details_BookingNotFound_ReturnsNotFound()
    {
        SetupProfile(UserId, ProfileId);
        _bookingService.Setup(x => x.GetByIdAsync(It.IsAny<Guid>(), default))
            .ReturnsAsync((BookingDto?)null);

        var controller = CreateController(UserId);
        var result = await controller.Details(Guid.NewGuid());
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Details_WithDifferentOwner_RedirectsToAccessDenied()
    {
        var bookingId = Guid.NewGuid();
        SetupProfile(UserId, ProfileId);
        _bookingService.Setup(x => x.GetByIdAsync(bookingId, default))
            .ReturnsAsync(new BookingDto
            {
                Id = bookingId,
                ClientProfileId = OtherProfileId,
                ProviderProfileId = OtherProfileId,
                Status = BookingStatus.Pending
            });
        _reviewService.Setup(x => x.GetByBookingAsync(bookingId, default))
            .ReturnsAsync((ReviewDto?)null);

        var controller = CreateController(UserId);
        var result = await controller.Details(bookingId);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("AccessDenied", redirect.ActionName);
    }

    [Fact]
    public async Task Details_WithCorrectOwner_ReturnsView()
    {
        var bookingId = Guid.NewGuid();
        SetupProfile(UserId, ProfileId);
        _bookingService.Setup(x => x.GetByIdAsync(bookingId, default))
            .ReturnsAsync(new BookingDto
            {
                Id = bookingId,
                ClientProfileId = ProfileId,
                ProviderProfileId = OtherProfileId,
                Status = BookingStatus.Pending,
                ListingTitle = "Test Listing"
            });
        _reviewService.Setup(x => x.GetByBookingAsync(bookingId, default))
            .ReturnsAsync((ReviewDto?)null);

        var controller = CreateController(UserId);
        var result = await controller.Details(bookingId);

        Assert.IsType<ViewResult>(result);
    }

    // --- Checkout GET ---

    [Fact]
    public async Task CheckoutGet_UserNotFound_ReturnsUnauthorized()
    {
        _userService.Setup(x => x.GetProfileByUserIdAsync(UserId, default))
            .ReturnsAsync((UserProfileDto?)null);

        var controller = CreateController(UserId);
        var result = await controller.Checkout(Guid.NewGuid());
        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public async Task CheckoutGet_WithDifferentOwner_RedirectsToAccessDenied()
    {
        var bookingId = Guid.NewGuid();
        SetupProfile(UserId, ProfileId);
        _bookingService.Setup(x => x.GetByIdAsync(bookingId, default))
            .ReturnsAsync(new BookingDto
            {
                Id = bookingId,
                ClientProfileId = OtherProfileId,
                Status = BookingStatus.AwaitingPayment
            });

        var controller = CreateController(UserId);
        var result = await controller.Checkout(bookingId);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("AccessDenied", redirect.ActionName);
    }

    [Fact]
    public async Task CheckoutGet_WithNonAwaitingPayment_RedirectsToDetails()
    {
        var bookingId = Guid.NewGuid();
        SetupProfile(UserId, ProfileId);
        _bookingService.Setup(x => x.GetByIdAsync(bookingId, default))
            .ReturnsAsync(new BookingDto
            {
                Id = bookingId,
                ClientProfileId = ProfileId,
                Status = BookingStatus.Confirmed
            });

        var controller = CreateController(UserId);
        var result = await controller.Checkout(bookingId);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Details", redirect.ActionName);
    }

    [Fact]
    public async Task CheckoutGet_WithAwaitingPayment_ReturnsCheckoutView()
    {
        var bookingId = Guid.NewGuid();
        SetupProfile(UserId, ProfileId);
        _bookingService.Setup(x => x.GetByIdAsync(bookingId, default))
            .ReturnsAsync(new BookingDto
            {
                Id = bookingId,
                ClientProfileId = ProfileId,
                Status = BookingStatus.AwaitingPayment,
                TotalPrice = 100m,
                ListingTitle = "Test Listing"
            });

        var controller = CreateController(UserId);
        var result = await controller.Checkout(bookingId);

        var viewResult = Assert.IsType<ViewResult>(result);
        var vm = Assert.IsType<CheckoutViewModel>(viewResult.Model);
        Assert.Equal(bookingId, vm.BookingId);
    }

    // --- Checkout POST ---

    [Fact]
    public async Task CheckoutPost_UserNotFound_ReturnsUnauthorized()
    {
        _userService.Setup(x => x.GetProfileByUserIdAsync(UserId, default))
            .ReturnsAsync((UserProfileDto?)null);

        var controller = CreateController(UserId);
        var result = await controller.Checkout(new CheckoutSubmitViewModel { BookingId = Guid.NewGuid(), Method = PaymentMethod.Card });
        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public async Task CheckoutPost_ValidPayment_RedirectsToReceipt()
    {
        var bookingId = Guid.NewGuid();
        var paymentId = Guid.NewGuid();
        SetupProfile(UserId, ProfileId);
        _clientPaymentService
            .Setup(x => x.PayAsync(ProfileId, It.IsAny<PayRequest>(), default))
            .ReturnsAsync(new PaymentReceiptDto(paymentId, bookingId, 100m, 5m, 105m, "Card", "Held", DateTime.UtcNow));

        var controller = CreateController(UserId);
        var model = new CheckoutSubmitViewModel { BookingId = bookingId, Method = PaymentMethod.Card };
        var result = await controller.Checkout(model);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Receipt", redirect.ActionName);
        Assert.Equal("Payments", redirect.ControllerName);
    }

    [Fact]
    public async Task CheckoutPost_PaymentServiceThrows_RedirectsToDetails()
    {
        var bookingId = Guid.NewGuid();
        SetupProfile(UserId, ProfileId);
        _clientPaymentService
            .Setup(x => x.PayAsync(ProfileId, It.IsAny<PayRequest>(), default))
            .ThrowsAsync(new BusinessRuleException("Cannot pay for this booking"));

        var controller = CreateController(UserId);
        // Need TempData for error message writing
        var tempDataMock = new Mock<Microsoft.AspNetCore.Mvc.ViewFeatures.ITempDataDictionary>();
        controller.TempData = tempDataMock.Object;

        var model = new CheckoutSubmitViewModel { BookingId = bookingId, Method = PaymentMethod.Card };
        var result = await controller.Checkout(model);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Details", redirect.ActionName);
    }

    // --- ConfirmCompletion ---

    [Fact]
    public async Task ConfirmCompletion_UserNotFound_ReturnsUnauthorized()
    {
        _userService.Setup(x => x.GetProfileByUserIdAsync(UserId, default))
            .ReturnsAsync((UserProfileDto?)null);

        var controller = CreateController(UserId);
        var result = await controller.ConfirmCompletion(Guid.NewGuid());
        Assert.IsType<UnauthorizedResult>(result);
    }

    [Fact]
    public async Task ConfirmCompletion_WithDifferentOwner_RedirectsToAccessDenied()
    {
        var bookingId = Guid.NewGuid();
        SetupProfile(UserId, ProfileId);
        _bookingService.Setup(x => x.GetByIdAsync(bookingId, default))
            .ReturnsAsync(new BookingDto
            {
                Id = bookingId,
                ClientProfileId = OtherProfileId,
                Status = BookingStatus.ProviderCompleted
            });

        var controller = CreateController(UserId);
        var result = await controller.ConfirmCompletion(bookingId);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("AccessDenied", redirect.ActionName);
    }

    [Fact]
    public async Task ConfirmCompletion_WhenNotProviderCompleted_RedirectsToDetails()
    {
        var bookingId = Guid.NewGuid();
        SetupProfile(UserId, ProfileId);
        _bookingService.Setup(x => x.GetByIdAsync(bookingId, default))
            .ReturnsAsync(new BookingDto
            {
                Id = bookingId,
                ClientProfileId = ProfileId,
                Status = BookingStatus.Confirmed
            });

        var controller = CreateController(UserId);
        var result = await controller.ConfirmCompletion(bookingId);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Details", redirect.ActionName);
        _bookingService.Verify(x => x.UpdateStatusAsync(It.IsAny<Guid>(), It.IsAny<BookingStatus>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ConfirmCompletion_WhenProviderCompleted_TransitionsToClientConfirmed()
    {
        var bookingId = Guid.NewGuid();
        SetupProfile(UserId, ProfileId);
        _bookingService.Setup(x => x.GetByIdAsync(bookingId, default))
            .ReturnsAsync(new BookingDto
            {
                Id = bookingId,
                ClientProfileId = ProfileId,
                Status = BookingStatus.ProviderCompleted
            });
        _bookingService
            .Setup(x => x.UpdateStatusAsync(bookingId, BookingStatus.ClientConfirmed, ProfileId, default))
            .ReturnsAsync(new BookingDto { Id = bookingId, Status = BookingStatus.ClientConfirmed });

        var controller = CreateController(UserId);
        var result = await controller.ConfirmCompletion(bookingId);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Details", redirect.ActionName);
        _bookingService.Verify(x => x.UpdateStatusAsync(bookingId, BookingStatus.ClientConfirmed, ProfileId, default), Times.Once);
    }
}
