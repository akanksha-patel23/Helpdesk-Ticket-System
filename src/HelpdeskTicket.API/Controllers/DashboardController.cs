using HelpdeskTicket.Application.DTOs.Dashboard;
using HelpdeskTicket.Application.Interfaces;
using HelpdeskTicket.Core.Wrappers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HelpdeskTicket.API.Controllers;

// TODO: Implement in step-by-step coding phase
[Authorize]
[ApiController]
[Route("api/dashboard")]
public class DashboardController : ControllerBase {
    private readonly IDashboardService _dashboardService;
    private readonly ILogger<DashboardController> _logger;

    public DashboardController(IDashboardService dashboardService, ILogger<DashboardController> logger)
    {
        _dashboardService = dashboardService;
        _logger = logger;
    }

    // GET api/dashboard?userId=1&roleId=1
    [HttpGet]
    public async Task<IActionResult> GetDashboardStats([FromQuery] int userId, [FromQuery] int roleId)
    {
        if (userId <= 0 || roleId <= 0)
            return BadRequest(ApiResponse<object>.Fail("Invalid userId or roleId."));

        var stats = await _dashboardService.GetDashboardStatsAsync(userId, roleId);
        return Ok(ApiResponse<DashboardStatsDto>.Ok(stats, "Dashboard data loaded."));
    }
}
