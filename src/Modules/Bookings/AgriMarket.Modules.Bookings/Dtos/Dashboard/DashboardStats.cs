using AgriMarket.Modules.Bookings.Enums;

namespace AgriMarket.Modules.Bookings.Dtos.Dashboard;

internal sealed class DashboardStats
{
    public int TotalUsers { get; set; }
    public int NewUsersThisMonth { get; set; }
    public int NewUsersThisWeek { get; set; }
    public int TotalListings { get; set; }
    public int ActiveListings { get; set; }
    public int InactiveListings { get; set; }
    public int TotalBookings { get; set; }
    public Dictionary<BookingStatus, int>? BookingsByStatus { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal TotalPlatformFees { get; set; }
    public decimal RevenueThisMonth { get; set; }
    public int ActiveDisputes { get; set; }
    public int ResolvedDisputes { get; set; }
    public List<RecentBookingDto>? RecentBookings { get; set; }
}

internal sealed class RecentBookingDto
{
    public Guid Id { get; set; }
    public int Status { get; set; }
    public decimal TotalPrice { get; set; }
    public decimal AreaInHectares { get; set; }
    public DateTime CreatedAt { get; set; }
    public ClientProfileDto? ClientProfile { get; set; }
    public ServiceListingDto? ServiceListing { get; set; }
}

internal sealed class ClientProfileDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
}

internal sealed class ServiceListingDto
{
    public string Title { get; set; } = string.Empty;
}
