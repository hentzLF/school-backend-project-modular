using AgriMarket.Modules.Bookings.Dtos.Dashboard;

namespace AgriMarket.Modules.Bookings.Services;

internal interface IDashboardService
{
    Task<DashboardStats> GetDashboardStatsAsync(CancellationToken ct = default);
}
