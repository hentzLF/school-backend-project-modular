using AgriMarket.Modules.Bookings.Dtos.Reviews;
using AgriMarket.Modules.Bookings.Entities;
using AgriMarket.Modules.Bookings.Enums;
using AgriMarket.Modules.Bookings.Persistence;
using AgriMarket.Tests.Helpers;
using FluentAssertions;
using Xunit;

namespace AgriMarket.Tests.Services;

public class ReviewServiceTests
{
    [Fact]
    public async Task GetByBookingAsync_NoReview_ReturnsNull()
    {
        var db = TestDbContextFactory.CreateBookingsDb();
        var service = TestServiceFactory.CreateReviewService(db);

        var result = await service.GetByBookingAsync(Guid.NewGuid());

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByBookingAsync_WithReview_ReturnsDto()
    {
        var db = TestDbContextFactory.CreateBookingsDb();
        var bookingId = Guid.NewGuid();
        var booking = new Booking
        {
            Id = bookingId, ClientProfileId = Guid.NewGuid(),
            ServiceListingId = Guid.NewGuid(), AvailabilityId = Guid.NewGuid(),
            Status = BookingStatus.ClientConfirmed, TotalPrice = 100, AreaInHectares = 1,
            CreatedAt = DateTime.UtcNow
        };
        db.Bookings.Add(booking);
        db.Reviews.Add(new Review
        {
            Id = Guid.NewGuid(), BookingId = bookingId,
            ReviewerProfileId = Guid.NewGuid(), ReviewedProfileId = Guid.NewGuid(),
            Rating = 5, Comment = "Great", CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var service = TestServiceFactory.CreateReviewService(db);
        var result = await service.GetByBookingAsync(bookingId);

        result.Should().NotBeNull();
        result!.Rating.Should().Be(5);
    }
}
