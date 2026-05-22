using AgriMarket.Modules.Bookings.Services;
using AgriMarket.Modules.Bookings.Enums;
using AgriMarket.Web.Areas.Admin.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgriMarket.Web.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Policy = "AdminOnly")]
public class DashboardController : Controller
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    public async Task<IActionResult> Index()
    {
        var stats = await _dashboardService.GetDashboardStatsAsync();

        var vm = new DashboardViewModel
        {
            TotalUsers = stats.TotalUsers,
            NewUsersThisMonth = stats.NewUsersThisMonth,
            NewUsersThisWeek = stats.NewUsersThisWeek,

            TotalListings = stats.TotalListings,
            ActiveListings = stats.ActiveListings,
            InactiveListings = stats.InactiveListings,

            TotalBookings = stats.TotalBookings,
            BookingsByStatus = stats.BookingsByStatus,

            TotalRevenue = stats.TotalRevenue,
            TotalPlatformFees = stats.TotalPlatformFees,
            RevenueThisMonth = stats.RevenueThisMonth,

            ActiveDisputes = stats.ActiveDisputes,
            ResolvedDisputes = stats.ResolvedDisputes,

            RecentBookings = stats.RecentBookings?.Select(b => new RecentBookingViewModel
            {
                Id = b.Id,
                ClientName = b.ClientProfile != null
                    ? $"{b.ClientProfile.FirstName} {b.ClientProfile.LastName}"
                    : "Unknown",
                ListingTitle = b.ServiceListing?.Title ?? "Unknown",
                Status = (BookingStatus)b.Status,
                TotalPrice = b.TotalPrice,
                CreatedAt = b.CreatedAt
            }) ?? []
        };

        return View(vm);
    }
}
