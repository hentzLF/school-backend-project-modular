using AgriMarket.Modules.Marketplace.Entities;
using AgriMarket.Modules.Marketplace.Persistence;
using AgriMarket.Modules.Marketplace.Services;
using AgriMarket.Tests.Helpers;
using FluentAssertions;
using Xunit;

namespace AgriMarket.Tests.Services;

public class LocationLookupServiceTests
{
    private static LocationLookupService CreateService(MarketplaceDbContext db)
    {
        return new LocationLookupService(
            new AgriMarket.Modules.Marketplace.Persistence.EfRepository<County>(db),
            new AgriMarket.Modules.Marketplace.Persistence.EfRepository<Municipality>(db));
    }

    [Fact]
    public async Task GetAllCountiesAsync_ReturnsSeededCounties()
    {
        var db = TestDbContextFactory.CreateMarketplaceDb();
        var service = CreateService(db);

        var result = await service.GetAllCountiesAsync();

        result.Should().HaveCount(15);
    }
}
