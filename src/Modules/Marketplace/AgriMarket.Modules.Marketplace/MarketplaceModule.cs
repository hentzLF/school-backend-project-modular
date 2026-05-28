using AgriMarket.Modules.Marketplace.Contracts;
using AgriMarket.Modules.Marketplace.Persistence;
using AgriMarket.Modules.Marketplace.Services;
using AgriMarket.Shared.Modules;
using AgriMarket.Shared.Persistence;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AgriMarket.Modules.Marketplace;

/// <summary>
/// Composition entry point for the Marketplace module — the single public type
/// in the module core (see design.md decision 6).
/// </summary>
public sealed class MarketplaceModule : IModule
{
    public void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("DefaultConnection is missing from configuration.");

        services.AddDbContext<MarketplaceDbContext>(options =>
            options.UseNpgsql(connectionString, npgsql =>
                npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "marketplace")));

        services.AddKeyedScoped<IUnitOfWork, EfUnitOfWork>("marketplace");
        services.AddKeyedScoped(typeof(IRepository<>), "marketplace", typeof(EfRepository<>));
        services.AddScoped<IQueryMaterializer, EfQueryMaterializer>();
        services.AddScoped<IListingRepository, EfListingRepository>();
        services.AddScoped<IAvailabilityRepository, EfAvailabilityRepository>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<ILocationLookupService, LocationLookupService>();
        services.AddScoped<IEquipmentService, EquipmentService>();
        services.AddScoped<IListingService, ListingService>();
        services.AddScoped<ICatalogModule, CatalogModuleApi>();
    }

    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        // Marketplace MVC controllers are discovered via the module assembly
        // registered as an application part in the bootstrapper. No
        // module-specific endpoints to map.
    }

    public async Task InitializeDatabaseAsync(
        IServiceProvider scopedServices,
        CancellationToken cancellationToken = default)
    {
        var context = scopedServices.GetRequiredService<MarketplaceDbContext>();
        await context.Database.MigrateAsync(cancellationToken);
    }
}
