using AgriMarket.Modules.Bookings.Enums;

namespace AgriMarket.Web.Areas.Admin.ViewModels;

internal class DashboardViewModel
{
    // Users
    public int TotalUsers { get; set; }
    public int NewUsersThisMonth { get; set; }
    public int NewUsersThisWeek { get; set; }

    // Listings
    public int TotalListings { get; set; }
    public int ActiveListings { get; set; }
    public int InactiveListings { get; set; }

    // Bookings
    public int TotalBookings { get; set; }
    public Dictionary<BookingStatus, int> BookingsByStatus { get; set; } = new();

    // Revenue
    public decimal TotalRevenue { get; set; }
    public decimal TotalPlatformFees { get; set; }
    public decimal RevenueThisMonth { get; set; }

    // Disputes
    public int ActiveDisputes { get; set; }
    public int ResolvedDisputes { get; set; }

    // Recent activity
    public IEnumerable<RecentBookingViewModel> RecentBookings { get; set; } = [];
}

internal class RecentBookingViewModel
{
    public Guid Id { get; set; }
    public string ClientName { get; set; } = default!;
    public string ListingTitle { get; set; } = default!;
    public BookingStatus Status { get; set; }
    public decimal TotalPrice { get; set; }
    public DateTime CreatedAt { get; set; }
}
