using HelpdeskTicket.Web.Helpers;
using HelpdeskTicket.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace HelpdeskTicket.Web.Controllers;

// TODO: Implement in step-by-step coding phase
public class DashboardController : Controller 
{
    private readonly ApiDashboardService _dashboardService;
    private readonly ILogger<DashboardController> _logger;

    public DashboardController(ApiDashboardService dashboardService, ILogger<DashboardController> logger)
    {
        _dashboardService = dashboardService;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        if (!SessionHelper.IsLoggedIn(HttpContext.Session))
            return RedirectToAction("Login", "Account");

        var stats = await _dashboardService.GetDashboardStatsAsync();

        if (stats is null)
        {
            _logger.LogWarning("Dashboard stats null for UserId:{Id}", SessionHelper.GetUserId(HttpContext.Session));
            TempData["ErrorMessage"] = "Could not load dashboard data. Please try again.";
            return View(new HelpdeskTicket.Application.DTOs.Dashboard.DashboardStatsDto());
        }

        return View(stats);
    }
}
