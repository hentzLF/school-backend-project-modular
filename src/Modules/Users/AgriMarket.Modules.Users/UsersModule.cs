using AgriMarket.Modules.Users.Contracts;
using AgriMarket.Modules.Users.Persistence;
using AgriMarket.Modules.Users.Persistence.Seeding;
using AgriMarket.Modules.Users.Security;
using AgriMarket.Modules.Users.Services;
using AgriMarket.Shared.Modules;
using AgriMarket.Shared.Persistence;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AgriMarket.Modules.Users;

/// <summary>
/// Composition entry point for the Users module. This is the single public
/// type in the module core — the deliberate composition seam the bootstrapper
/// instantiates (see design.md decision 6).
/// </summary>
public sealed class UsersModule : IModule
{
    public void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("DefaultConnection is missing from configuration.");

        services.AddDbContext<UsersDbContext>(options =>
            options.UseNpgsql(connectionString, npgsql =>
                npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "users")));

        services.AddScoped<IUnitOfWork, EfUnitOfWork>();
        services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));
        services.AddScoped<IAppUserRepository, EfAppUserRepository>();
        services.AddScoped<IRefreshTokenRepository, EfRefreshTokenRepository>();
        services.AddScoped<IUserProfileRepository, EfUserProfileRepository>();
        services.AddScoped<IPasswordHasher, BCryptPasswordHasher>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IUsersModule, UsersModuleApi>();
    }

    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        // The Users module exposes MVC controllers. They are discovered through
        // the module assembly registered as an MVC application part in the
        // bootstrapper and mapped by the bootstrapper's global MapControllers().
        // There are no module-specific (minimal-API or hub) endpoints to map.
    }

    public async Task InitializeDatabaseAsync(
        IServiceProvider scopedServices,
        CancellationToken cancellationToken = default)
    {
        var context = scopedServices.GetRequiredService<UsersDbContext>();
        await context.Database.MigrateAsync(cancellationToken);

        var passwordHasher = scopedServices.GetRequiredService<IPasswordHasher>();
        await UsersDbSeeder.SeedAsync(context, passwordHasher, cancellationToken);
    }
}
