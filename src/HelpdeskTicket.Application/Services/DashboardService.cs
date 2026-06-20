using HelpdeskTicket.Application.DTOs.Dashboard;
using HelpdeskTicket.Application.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelpdeskTicket.Application.Services;

public class DashboardService : IDashboardService
{
    private readonly IDashboardRepository _repo;
    private readonly ILogger<DashboardService> _logger;

    public DashboardService(IDashboardRepository repo, ILogger<DashboardService> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    public async Task<DashboardStatsDto> GetDashboardStatsAsync(int requestedById, int requestedRoleId)
    {
        _logger.LogInformation("GetDashboardStats for UserId:{Id} RoleId:{Role}", requestedById, requestedRoleId);
        return await _repo.GetDashboardStatsAsync(requestedById, requestedRoleId);
    }
}
