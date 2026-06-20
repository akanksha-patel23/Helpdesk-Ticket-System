namespace HelpdeskTicket.Application.DTOs.Dashboard;

// TODO: Implement in step-by-step coding phase
public class RecentTicketDto {
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string StatusName { get; set; } = string.Empty;
    public string PriorityName { get; set; } = string.Empty;
    public string AssignedToName { get; set; } = string.Empty;
    public DateTime CreatedDateTime { get; set; }
    }
