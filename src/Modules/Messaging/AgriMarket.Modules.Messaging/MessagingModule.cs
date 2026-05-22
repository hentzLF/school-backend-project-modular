using AgriMarket.Modules.Messaging.Persistence;
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

        services.AddScoped<IUnitOfWork, EfUnitOfWork>();
        services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));
        services.AddScoped<IQueryMaterializer, EfQueryMaterializer>();

        // MessagingService, EfConversationRepository, the SignalR MessageHub and
        // the IMessagingModule adapter are added in Phase 6 — they require
        // cross-module participant-name resolution via IUsersModule.
    }

    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        // The SignalR MessageHub is mapped by the bootstrapper in Phase 6.
    }
}
