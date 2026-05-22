using AgriMarket.Modules.Marketplace.Dtos.Equipment;
using AgriMarket.Modules.Marketplace.Enums;
using AgriMarket.Tests.Helpers;
using FluentAssertions;
using Xunit;

namespace AgriMarket.Tests.Services;

public class EquipmentServiceTests
{
    [Fact]
    public async Task GetByProviderAsync_NoEquipment_ReturnsEmpty()
    {
        var db = TestDbContextFactory.CreateMarketplaceDb();
        var service = TestServiceFactory.CreateEquipmentService(db);

        var result = await service.GetByProviderAsync(Guid.NewGuid());

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByProviderAsync_WithEquipment_ReturnsList()
    {
        var db = TestDbContextFactory.CreateMarketplaceDb();
        var profileId = Guid.NewGuid();
        TestDbContextFactory.SeedEquipment(db, profileId);

        var service = TestServiceFactory.CreateEquipmentService(db);
        var result = await service.GetByProviderAsync(profileId);

        result.Should().HaveCount(1);
    }
}
