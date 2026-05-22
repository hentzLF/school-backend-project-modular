using AgriMarket.Modules.Bookings.Dtos.Dashboard;
using AgriMarket.Modules.Bookings.Entities;
using AgriMarket.Modules.Bookings.Enums;
using AgriMarket.Modules.Marketplace.Contracts;
using AgriMarket.Shared.Persistence;

namespace AgriMarket.Modules.Bookings.Services;

internal sealed class ProviderDashboardService(
    IRepository<Booking> bookings,
    IRepository<Payment> payments,
    IQueryMaterializer mat,
    ICatalogModule catalog) : IProviderDashboardService
{
    private static readonly BookingStatus[] ActiveStatuses =
    [
        BookingStatus.Pending,
        BookingStatus.AwaitingPayment,
        BookingStatus.Confirmed,
        BookingStatus.InProgress
    ];

    public async Task<ProviderDashboardDto> GetStatsAsync(Guid providerProfileId, CancellationToken ct = default)
    {
        var listings = await catalog.GetListingsByProviderAsync(providerProfileId, ct);
        var listingIds = listings.Select(l => l.Id).ToList();

        var providerBookings = bookings.Query().Where(b => listingIds.Contains(b.ServiceListingId));

        var activeBookings = await mat.CountAsync(
            providerBookings.Where(b => ActiveStatuses.Contains(b.Status)), ct);
        var completedBookings = await mat.CountAsync(
            providerBookings.Where(b => b.Status == BookingStatus.ClientConfirmed), ct);
        var cancelledBookings = await mat.CountAsync(
            providerBookings.Where(b => b.Status == BookingStatus.Cancelled), ct);

        var providerPayments = payments.Query()
            .Where(p => listingIds.Contains(p.Booking!.ServiceListingId));

        var totalEarnings = await mat.SumAsync(
            providerPayments.Where(p => p.Status == PaymentStatus.Released), p => (decimal?)p.Amount, ct);
        var moneyHeld = await mat.SumAsync(
            providerPayments.Where(p => p.Status == PaymentStatus.Held), p => (decimal?)p.Amount, ct);

        return new ProviderDashboardDto
        {
            TotalEarnings = totalEarnings,
            MoneyHeld = moneyHeld,
            ActiveBookings = activeBookings,
            CompletedBookings = completedBookings,
            CancelledBookings = cancelledBookings,
            ActiveListings = listings.Count(l => l.IsActive),
            TotalListings = listings.Count
        };
    }
}
