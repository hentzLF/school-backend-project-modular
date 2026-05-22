using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AgriMarket.Shared.Modules;

/// <summary>
/// Composition contract every module exposes to the bootstrapper. A module owns
/// its service registrations, its endpoints and its database lifecycle.
/// </summary>
public interface IModule
{
    /// <summary>Registers the module's services, DbContext and adapters.</summary>
    void RegisterServices(IServiceCollection services, IConfiguration configuration);

    /// <summary>Maps the module's own (non-controller) endpoints, if any.</summary>
    void MapEndpoints(IEndpointRouteBuilder endpoints);

    /// <summary>
    /// Applies the module's pending migrations and runs its seeders. Invoked by
    /// the host once at startup with a scoped <see cref="IServiceProvider"/>.
    /// </summary>
    Task InitializeDatabaseAsync(IServiceProvider scopedServices, CancellationToken cancellationToken = default);
}
