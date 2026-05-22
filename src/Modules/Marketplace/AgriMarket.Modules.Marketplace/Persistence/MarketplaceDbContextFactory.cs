using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AgriMarket.Modules.Marketplace.Persistence;

internal sealed class MarketplaceDbContextFactory : IDesignTimeDbContextFactory<MarketplaceDbContext>
{
    public MarketplaceDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<MarketplaceDbContext>()
            .UseNpgsql("Host=localhost;Database=agrimarket;Username=postgres;Password=postgres",
                b => b.MigrationsHistoryTable("__EFMigrationsHistory", "marketplace"))
            .Options;

        return new MarketplaceDbContext(options);
    }
}
