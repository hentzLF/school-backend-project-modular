using AgriMarket.Modules.Bookings.Contracts;
using AgriMarket.Modules.Bookings.Persistence;
using AgriMarket.Modules.Bookings.Services;
using AgriMarket.Shared.Modules;
using AgriMarket.Shared.Persistence;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AgriMarket.Modules.Bookings;

/// <summary>
/// Composition entry point for the Bookings module — the single public type in
/// the module core (see design.md decision 6).
/// </summary>
public sealed class BookingsModule : IModule
{
    public void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("DefaultConnection is missing from configuration.");

        services.AddDbContext<BookingsDbContext>(options =>
            options.UseNpgsql(connectionString, npgsql =>
                npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "bookings")));

        services.AddScoped<IUnitOfWork, EfUnitOfWork>();
        services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));
        services.AddScoped<IQueryMaterializer, EfQueryMaterializer>();
        services.AddScoped<IBookingRepository, EfBookingRepository>();
        services.AddScoped<IPaymentRepository, EfPaymentRepository>();
        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<IBookingsModule, BookingsModuleApi>();
    }

    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        // Bookings MVC controllers are discovered via the module assembly
        // registered as an application part in the bootstrapper. No
        // module-specific endpoints to map.
    }
}
