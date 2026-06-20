namespace HelpdeskTicket.Application.DTOs.Dashboard;

// TODO: Implement in step-by-step coding phase
public class DashboardStatsDto {
    // Summary counters — top cards
    public int TotalTickets { get; set; }
    public int OpenTickets { get; set; }
    public int AssignedTickets { get; set; }
    public int ResolvedTickets { get; set; }
    public int ClosedTickets { get; set; }
    public int CriticalTickets { get; set; }
    public int InProgressTickets { get; set; }

    // Recent tickets grid
    public List<RecentTicketDto> RecentTickets { get; set; } = [];
    // Chart.js data
    public List<ChartDataDto> StatusChartData { get; set; } = [];
    public List<ChartDataDto> PriorityChartData { get; set; } = [];
}
