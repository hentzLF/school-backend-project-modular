using AgriMarket.Modules.Messaging.Contracts;
using AgriMarket.Modules.Messaging.Persistence;
using AgriMarket.Modules.Messaging.Services;
using AgriMarket.Shared.Modules;
using AgriMarket.Shared.Persistence;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AgriMarket.Modules.Messaging;

/// <summary>
/// Composition entry point for the Messaging module — the single public type in
/// the module core (see design.md decision 6).
/// </summary>
public sealed class MessagingModule : IModule
{
    public void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("DefaultConnection is missing from configuration.");

        services.AddDbContext<MessagingDbContext>(options =>
            options.UseNpgsql(connectionString, npgsql =>
                npgsql.MigrationsHistoryTable("__EFMigrationsHistory", "messaging")));

        services.AddKeyedScoped<IUnitOfWork, EfUnitOfWork>("messaging");
        services.AddKeyedScoped(typeof(IRepository<>), typeof(EfRepository<>), "messaging");
        services.AddScoped<IQueryMaterializer, EfQueryMaterializer>();
        services.AddScoped<IConversationRepository, EfConversationRepository>();
        services.AddScoped<IMessagingService, MessagingService>();
        services.AddScoped<IMessagingModule, MessagingModuleApi>();

        // The SignalR-backed IMessageNotifier implementation is registered by
        // the bootstrapper (it owns the SignalR hub).
    }

    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        // The SignalR MessageHub is mapped by the bootstrapper in Phase 6.
    }

    public async Task InitializeDatabaseAsync(
        IServiceProvider scopedServices,
        CancellationToken cancellationToken = default)
    {
        var context = scopedServices.GetRequiredService<MessagingDbContext>();
        await context.Database.MigrateAsync(cancellationToken);
    }
}
