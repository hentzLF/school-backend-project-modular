using AgriMarket.Modules.Bookings.Contracts;
using AgriMarket.Modules.Marketplace.Entities;
using AgriMarket.Modules.Marketplace.Persistence;
using AgriMarket.Modules.Marketplace.Services;
using AgriMarket.Modules.Users.Contracts;
using AgriMarket.Shared.Persistence;
using AgriMarket.Tests.Helpers;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace AgriMarket.Tests.Services;

public class ListingServiceLocationTests
{
    private static ListingService CreateService(MarketplaceDbContext db)
    {
        return new ListingService(
            new EfListingRepository(db),
            new EfAvailabilityRepository(db),
            new AgriMarket.Modules.Marketplace.Persistence.EfRepository<Location>(db),
            new AgriMarket.Modules.Marketplace.Persistence.EfRepository<Municipality>(db),
            new AgriMarket.Modules.Marketplace.Persistence.EfUnitOfWork(db),
            Mock.Of<IUsersModule>(),
            Mock.Of<IBookingsModule>(),
            NullLogger<ListingService>.Instance);
    }

    [Fact]
    public async Task GetAllAsync_NoListings_ReturnsEmpty()
    {
        var db = TestDbContextFactory.CreateMarketplaceDb();
        var service = CreateService(db);

        var result = await service.GetAllAsync();

        result.Should().BeEmpty();
    }
}
