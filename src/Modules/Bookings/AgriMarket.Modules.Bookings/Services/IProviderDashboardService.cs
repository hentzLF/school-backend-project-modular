using AgriMarket.Modules.Bookings.Dtos.Dashboard;

namespace AgriMarket.Modules.Bookings.Services;

public interface IProviderDashboardService
{
    Task<ProviderDashboardDto> GetStatsAsync(Guid providerProfileId, CancellationToken ct = default);
}
