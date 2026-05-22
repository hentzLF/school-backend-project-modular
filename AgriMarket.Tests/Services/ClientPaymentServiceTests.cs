using AgriMarket.Modules.Bookings.Dtos.Payments;
using AgriMarket.Modules.Bookings.Entities;
using AgriMarket.Modules.Bookings.Enums;
using AgriMarket.Modules.Bookings.Persistence;
using AgriMarket.Modules.Bookings.Services;
using AgriMarket.Modules.Marketplace.Contracts;
using AgriMarket.Shared.Exceptions;
using AgriMarket.Shared.Persistence;
using FluentAssertions;
using MediatR;
using Moq;
using Xunit;

namespace AgriMarket.Tests.Services;

public class ClientPaymentServiceTests
{
    private readonly Mock<IBookingRepository> _bookingRepo = new();
    private readonly Mock<IRepository<Payment>> _paymentRepo = new();
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly Mock<IQueryMaterializer> _mat = new();
    private readonly Mock<ICatalogModule> _catalog = new();
    private readonly Mock<IMediator> _mediator = new();
    private readonly ClientPaymentService _sut;

    private static readonly Guid ClientProfileId = Guid.NewGuid();
    private static readonly Guid OtherProfileId = Guid.NewGuid();
    private static readonly Guid BookingId = Guid.NewGuid();
    private static readonly Guid ListingId = Guid.NewGuid();

    public ClientPaymentServiceTests()
    {
        _catalog
            .Setup(c => c.GetListingSummaryAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ListingSummaryDto?)null);

        _sut = new ClientPaymentService(
            _bookingRepo.Object,
            _paymentRepo.Object,
            _uow.Object,
            _mat.Object,
            _catalog.Object,
            _mediator.Object);
    }

    private static Booking CreateAwaitingPaymentBooking(
        Guid? clientProfileId = null,
        decimal totalPrice = 200m) =>
        new()
        {
            Id = BookingId,
            ClientProfileId = clientProfileId ?? ClientProfileId,
            ServiceListingId = ListingId,
            Status = BookingStatus.AwaitingPayment,
            TotalPrice = totalPrice
        };

    private void SetupBookingForUpdate(Booking? booking)
    {
        _bookingRepo
            .Setup(r => r.GetForUpdateAsync(BookingId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(booking);
    }

    private static PayRequest ValidRequest(PaymentMethod method = PaymentMethod.Card) =>
        new(BookingId, method);

    // -------------------------------------------------------------------------
    // PayAsync — happy path
    // -------------------------------------------------------------------------

    [Fact]
    public async Task PayAsync_ValidRequest_CreatesPaymentAndReturnsReceipt()
    {
        // Arrange
        var booking = CreateAwaitingPaymentBooking();
        SetupBookingForUpdate(booking);

        // Act
        var result = await _sut.PayAsync(ClientProfileId, ValidRequest());

        // Assert
        result.Should().NotBeNull();
        result.BookingId.Should().Be(BookingId);
        result.Amount.Should().Be(200m);
        result.Method.Should().Be("Card");
        result.Status.Should().Be("Held");
        result.PaymentId.Should().NotBeEmpty();
        _paymentRepo.Verify(r => r.Add(It.IsAny<Payment>()), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task PayAsync_ValidRequest_SetsBookingStatusToConfirmed()
    {
        // Arrange
        var booking = CreateAwaitingPaymentBooking();
        SetupBookingForUpdate(booking);

        // Act
        await _sut.PayAsync(ClientProfileId, ValidRequest());

        // Assert
        booking.Status.Should().Be(BookingStatus.Confirmed);
    }

    [Fact]
    public async Task PayAsync_ValidRequest_CalculatesPlatformFeeCorrectly()
    {
        // Arrange
        const decimal totalPrice = 400m;
        const decimal expectedFee = totalPrice * 0.05m;
        var booking = CreateAwaitingPaymentBooking(totalPrice: totalPrice);
        SetupBookingForUpdate(booking);

        // Act
        var result = await _sut.PayAsync(ClientProfileId, ValidRequest());

        // Assert
        result.PlatformFee.Should().Be(expectedFee);
        result.Amount.Should().Be(totalPrice);
    }

    // -------------------------------------------------------------------------
    // PayAsync — error paths
    // -------------------------------------------------------------------------

    [Fact]
    public async Task PayAsync_BookingNotFound_ThrowsKeyNotFoundException()
    {
        // Arrange
        SetupBookingForUpdate(null);

        // Act
        var act = () => _sut.PayAsync(ClientProfileId, ValidRequest());

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"Booking {BookingId} not found.");
    }

    [Fact]
    public async Task PayAsync_CallerNotClient_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var booking = CreateAwaitingPaymentBooking(clientProfileId: OtherProfileId);
        SetupBookingForUpdate(booking);

        // Act
        var act = () => _sut.PayAsync(ClientProfileId, ValidRequest());

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("You are not the client of this booking.");
    }

    [Theory]
    [InlineData(BookingStatus.Pending)]
    [InlineData(BookingStatus.Confirmed)]
    [InlineData(BookingStatus.InProgress)]
    [InlineData(BookingStatus.ProviderCompleted)]
    [InlineData(BookingStatus.ClientConfirmed)]
    [InlineData(BookingStatus.Archived)]
    [InlineData(BookingStatus.Cancelled)]
    [InlineData(BookingStatus.Disputed)]
    public async Task PayAsync_BookingNotAwaitingPayment_ThrowsBusinessRuleException(BookingStatus status)
    {
        // Arrange
        var booking = CreateAwaitingPaymentBooking();
        booking.Status = status;
        SetupBookingForUpdate(booking);

        // Act
        var act = () => _sut.PayAsync(ClientProfileId, ValidRequest());

        // Assert
        await act.Should().ThrowAsync<BusinessRuleException>()
            .WithMessage("Booking is not in a payable state.");
    }

    [Fact]
    public async Task PayAsync_InvalidMethod_ThrowsBusinessRuleException()
    {
        // Arrange
        var booking = CreateAwaitingPaymentBooking();
        SetupBookingForUpdate(booking);
        var undefinedMethod = (PaymentMethod)99;

        // Act
        var act = () => _sut.PayAsync(ClientProfileId, new PayRequest(BookingId, undefinedMethod));

        // Assert
        await act.Should().ThrowAsync<BusinessRuleException>()
            .WithMessage("Invalid payment method.");
    }

    // -------------------------------------------------------------------------
    // PayAsync — boundary: all valid payment methods
    // -------------------------------------------------------------------------

    [Theory]
    [InlineData(PaymentMethod.Card, "Card")]
    [InlineData(PaymentMethod.BankTransfer, "BankTransfer")]
    [InlineData(PaymentMethod.Cash, "Cash")]
    public async Task PayAsync_EachValidMethod_ReturnsMatchingMethodInReceipt(
        PaymentMethod method,
        string expectedMethodName)
    {
        // Arrange
        var booking = CreateAwaitingPaymentBooking();
        SetupBookingForUpdate(booking);

        // Act
        var result = await _sut.PayAsync(ClientProfileId, new PayRequest(BookingId, method));

        // Assert
        result.Method.Should().Be(expectedMethodName);
    }

}
