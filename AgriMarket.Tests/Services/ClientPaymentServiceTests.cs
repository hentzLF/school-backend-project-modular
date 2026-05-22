using AgriMarket.Modules.Bookings.Dtos.Payments;
using AgriMarket.Modules.Bookings.Entities;
using AgriMarket.Modules.Bookings.Enums;
using AgriMarket.Modules.Bookings.Persistence;
using AgriMarket.Shared.Exceptions;
using AgriMarket.Tests.Helpers;
using FluentAssertions;
using Xunit;

namespace AgriMarket.Tests.Services;

public class ClientPaymentServiceTests
{
    [Fact]
    public async Task PayAsync_BookingNotFound_ThrowsException()
    {
        var db = TestDbContextFactory.CreateBookingsDb();
        var service = TestServiceFactory.CreateClientPaymentService(db);

        var act = () => service.PayAsync(Guid.NewGuid(),
            new PayRequest(Guid.NewGuid(), PaymentMethod.Card));

        await act.Should().ThrowAsync<BusinessRuleException>();
    }
}
