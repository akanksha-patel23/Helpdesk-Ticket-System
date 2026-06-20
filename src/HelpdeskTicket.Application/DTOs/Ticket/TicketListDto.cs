namespace HelpdeskTicket.Application.DTOs.Ticket;

// TODO: Implement in step-by-step coding phase
public class TicketListDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string StatusName { get; set; } = string.Empty;
    public string PriorityName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string CreatedByName { get; set; } = string.Empty;
    public string AssignedToName { get; set; } = string.Empty;
    public DateTime CreatedDateTime { get; set; }
    public DateTime? UpdatedDateTime { get; set; }
    public int TotalCount { get; set; }  // inline count from SP for pagination
}
