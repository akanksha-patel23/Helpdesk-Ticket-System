using HelpdeskTicket.Application.Data;
using HelpdeskTicket.Application.DTOs.Dashboard;
using HelpdeskTicket.Application.Interfaces;
using HelpdeskTicket.Core.Constants;
using HelpdeskTicket.Core.Helpers;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelpdeskTicket.Application.Repositories;

public class DashboardRepository : IDashboardRepository
{
    private readonly IDbConnectionFactory _db;
    private readonly ILogger<DashboardRepository> _logger;

    public DashboardRepository(IDbConnectionFactory db, ILogger<DashboardRepository> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<DashboardStatsDto> GetDashboardStatsAsync(int requestedById, int requestedRoleId)
    {
        var result = new DashboardStatsDto();

        try
        {
            await using var conn = await _db.CreateConnectionAsync();
            using var cmd = SqlHelper.CreateCommand(conn, StoredProcedures.GetDashboardStats);
            cmd.Parameters.Add(SqlHelper.ParamInt("@RequestedById", requestedById));
            cmd.Parameters.Add(SqlHelper.ParamInt("@RequestedRoleId", requestedRoleId));

            await using var reader = await cmd.ExecuteReaderAsync();

            // ── Result Set 0: Summary counters ────────────────
            if (await reader.ReadAsync())
            {
                result.TotalTickets = SqlHelper.GetInt(reader, "TotalTickets");
                result.OpenTickets = SqlHelper.GetInt(reader, "OpenTickets");
                result.AssignedTickets = SqlHelper.GetInt(reader, "AssignedTickets");
                result.ResolvedTickets = SqlHelper.GetInt(reader, "ResolvedTickets");
                result.ClosedTickets = SqlHelper.GetInt(reader, "ClosedTickets");
                //result.CriticalTickets = SqlHelper.GetInt(reader, "CriticalTickets");
                result.InProgressTickets = SqlHelper.GetInt(reader, "InProgressTickets");
            }

            // ── Result Set 1: Recent 10 tickets ───────────────
            await reader.NextResultAsync();
            while (await reader.ReadAsync())
            {
                result.RecentTickets.Add(new RecentTicketDto
                {
                    Id = SqlHelper.GetInt(reader, "Id"),
                    Title = SqlHelper.GetString(reader, "Title"),
                    StatusName = SqlHelper.GetString(reader, "StatusName"),
                    PriorityName = SqlHelper.GetString(reader, "PriorityName"),
                    AssignedToName = SqlHelper.GetNullableString(reader, "AssignedToName") ?? "Unassigned",
                    //CreatedDateTime = SqlHelper.GetDateTime(reader, "CreatedDateTime")
                    CreatedDateTime = DateTimeHelper.ToIst(SqlHelper.GetDateTime(reader, "CreatedDateTime"))
                });
            }

            // ── Result Set 2: Status chart data (pie) ─────────
            await reader.NextResultAsync();
            while (await reader.ReadAsync())
            {
                result.StatusChartData.Add(new ChartDataDto
                {
                    Label = SqlHelper.GetString(reader, "Label"),
                    Count = SqlHelper.GetInt(reader, "Count")
                });
            }

            // ── Result Set 3: Priority chart data (bar) ───────
            await reader.NextResultAsync();
            while (await reader.ReadAsync())
            {
                result.PriorityChartData.Add(new ChartDataDto
                {
                    Label = SqlHelper.GetString(reader, "Label"),
                    Count = SqlHelper.GetInt(reader, "Count")
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetDashboardStatsAsync failed for UserId:{UserId} RoleId:{RoleId}",
                requestedById, requestedRoleId);
            throw;
        }

        return result;
    }
}
