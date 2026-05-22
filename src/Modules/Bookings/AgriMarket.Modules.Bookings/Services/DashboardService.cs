using AgriMarket.Modules.Bookings.Dtos.Dashboard;
using AgriMarket.Modules.Bookings.Entities;
using AgriMarket.Modules.Bookings.Enums;
using AgriMarket.Modules.Marketplace.Contracts;
using AgriMarket.Modules.Users.Contracts;
using AgriMarket.Shared.Persistence;

namespace AgriMarket.Modules.Bookings.Services;

internal sealed class DashboardService(
    IRepository<Booking> bookings,
    IRepository<Payment> payments,
    IQueryMaterializer mat,
    IUsersModule users,
    ICatalogModule catalog) : IDashboardService
{
    private const int RecentBookingsCount = 10;

    public async Task<DashboardStats> GetDashboardStatsAsync(CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var startOfMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var startOfWeek = now.AddDays(-7);

        var totalUsers = await users.CountUsersAsync(null, ct);
        var newUsersThisMonth = await users.CountUsersAsync(startOfMonth, ct);
        var newUsersThisWeek = await users.CountUsersAsync(startOfWeek, ct);

        var totalListings = await catalog.CountListingsAsync(null, ct);
        var activeListings = await catalog.CountListingsAsync(true, ct);
        var inactiveListings = await catalog.CountListingsAsync(false, ct);

        var totalBookings = await bookings.CountAsync(_ => true, ct);
        var statusCounts = await mat.ToListAsync(
            bookings.Query()
                .GroupBy(b => b.Status)
                .Select(g => new StatusCount(g.Key, g.Count())), ct);

        var bookingsByStatus = Enum.GetValues<BookingStatus>()
            .ToDictionary(s => s, s => statusCounts.FirstOrDefault(c => c.Status == s)?.Count ?? 0);

        var totalRevenue = await mat.SumAsync(payments.Query(), p => (decimal?)p.Amount, ct);
        var totalPlatformFees = await mat.SumAsync(payments.Query(), p => (decimal?)p.PlatformFee, ct);
        var revenueThisMonth = await mat.SumAsync(
            payments.Query().Where(p => p.CreatedAt >= startOfMonth), p => (decimal?)p.Amount, ct);

        var activeDisputes = await payments.CountAsync(p => p.Status == PaymentStatus.Disputed, ct);
        var resolvedDisputes = await payments.CountAsync(
            p => p.Status == PaymentStatus.Released || p.Status == PaymentStatus.Refunded, ct);

        var recentBookings = await BuildRecentBookingsAsync(ct);

        return new DashboardStats
        {
            TotalUsers = totalUsers,
            NewUsersThisMonth = newUsersThisMonth,
            NewUsersThisWeek = newUsersThisWeek,
            TotalListings = totalListings,
            ActiveListings = activeListings,
            InactiveListings = inactiveListings,
            TotalBookings = totalBookings,
            BookingsByStatus = bookingsByStatus,
            TotalRevenue = totalRevenue,
            TotalPlatformFees = totalPlatformFees,
            RevenueThisMonth = revenueThisMonth,
            ActiveDisputes = activeDisputes,
            ResolvedDisputes = resolvedDisputes,
            RecentBookings = recentBookings
        };
    }

    private async Task<List<RecentBookingDto>> BuildRecentBookingsAsync(CancellationToken ct)
    {
        var recent = await mat.ToListAsync(
            bookings.Query().OrderByDescending(b => b.CreatedAt).Take(RecentBookingsCount), ct);
        if (recent.Count == 0)
            return [];

        var clients = await users.GetProfilesAsync(
            recent.Select(b => b.ClientProfileId).Distinct().ToList(), ct);
        var listings = await catalog.GetListingSummariesAsync(
            recent.Select(b => b.ServiceListingId).Distinct().ToList(), ct);

        return recent.Select(b =>
        {
            clients.TryGetValue(b.ClientProfileId, out var client);
            listings.TryGetValue(b.ServiceListingId, out var listing);
            return new RecentBookingDto
            {
                Id = b.Id,
                Status = (int)b.Status,
                TotalPrice = b.TotalPrice,
                AreaInHectares = b.AreaInHectares,
                CreatedAt = b.CreatedAt,
                ClientProfile = client is null
                    ? null
                    : new ClientProfileDto { FirstName = client.FirstName, LastName = client.LastName },
                ServiceListing = listing is null
                    ? null
                    : new ServiceListingDto { Title = listing.Title }
            };
        }).ToList();
    }

    private sealed record StatusCount(BookingStatus Status, int Count);
}
