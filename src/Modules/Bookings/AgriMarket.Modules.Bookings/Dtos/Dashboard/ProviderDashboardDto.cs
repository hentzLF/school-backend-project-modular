namespace AgriMarket.Modules.Bookings.Dtos.Dashboard;

public sealed class ProviderDashboardDto
{
    public decimal TotalEarnings { get; init; }
    public decimal MoneyHeld { get; init; }
    public int ActiveBookings { get; init; }
    public int CompletedBookings { get; init; }
    public int CancelledBookings { get; init; }
    public int ActiveListings { get; init; }
    public int TotalListings { get; init; }
}
