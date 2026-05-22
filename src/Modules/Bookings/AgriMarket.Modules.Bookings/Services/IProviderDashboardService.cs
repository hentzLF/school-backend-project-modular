using AgriMarket.Modules.Bookings.Dtos.Dashboard;

namespace AgriMarket.Modules.Bookings.Services;

internal interface IProviderDashboardService
{
    Task<ProviderDashboardDto> GetStatsAsync(Guid providerProfileId, CancellationToken ct = default);
}
