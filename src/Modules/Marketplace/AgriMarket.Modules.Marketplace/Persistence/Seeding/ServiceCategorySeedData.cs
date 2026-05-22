using AgriMarket.Modules.Marketplace.Entities;

namespace AgriMarket.Modules.Marketplace.Persistence.Seeding;

/// <summary>
/// Fixed set of agricultural service categories, seeded via EF Core
/// <c>HasData</c> so they ship with the Marketplace module's migrations.
/// </summary>
internal static class ServiceCategorySeedData
{
    public static ServiceCategory[] GetAll() =>
    [
        new() { Id = new Guid("a1b2c3d4-0001-0000-0000-000000000001"), Name = "Hay Baling", Description = "Round and square baling services" },
        new() { Id = new Guid("a1b2c3d4-0002-0000-0000-000000000002"), Name = "Combine Harvesting", Description = "Grain and cereal harvesting" },
        new() { Id = new Guid("a1b2c3d4-0003-0000-0000-000000000003"), Name = "Spraying", Description = "Crop protection and fertilizer spraying" },
        new() { Id = new Guid("a1b2c3d4-0004-0000-0000-000000000004"), Name = "Soil Preparation", Description = "Ploughing, discing, and cultivating" },
        new() { Id = new Guid("a1b2c3d4-0005-0000-0000-000000000005"), Name = "Seeding", Description = "Precision and broadcast seeding" },
        new() { Id = new Guid("a1b2c3d4-0006-0000-0000-000000000006"), Name = "Mowing", Description = "Grass and hay mowing services" },
        new() { Id = new Guid("a1b2c3d4-0007-0000-0000-000000000007"), Name = "Transport", Description = "Agricultural cargo transport" },
    ];
}
