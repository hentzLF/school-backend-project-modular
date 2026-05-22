using AgriMarket.Modules.Bookings.Dtos.Dashboard;
using AgriMarket.Modules.Bookings.Services;
using AgriMarket.Shared.Web;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgriMarket.Modules.Bookings.Controllers.Admin;

[ApiController]
[ApiVersion("1")]
[Route("api/v{version:apiVersion}/admin/dashboard")]
[Authorize(Policy = "AdminOnly")]
internal sealed class AdminDashboardController(IDashboardService dashboardService) : ApiControllerBase
{
    private readonly IDashboardService _dashboardService = dashboardService;

    [HttpGet]
    [ProducesResponseType(typeof(DashboardStats), 200)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<IActionResult> Get()
    {
        var stats = await _dashboardService.GetDashboardStatsAsync();
        return Ok(stats);
    }
}
