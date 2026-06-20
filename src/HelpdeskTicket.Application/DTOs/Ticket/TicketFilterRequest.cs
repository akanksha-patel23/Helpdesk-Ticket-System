namespace HelpdeskTicket.Application.DTOs.Ticket;

// TODO: Implement in step-by-step coding phase
public class TicketFilterRequest
{
    public string? SearchText { get; set; }
    public int? StatusId { get; set; }
    public int? PriorityId { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;

    // Set server-side from session — never from client
    public int RequestedById { get; set; }
    public int RequestedRoleId { get; set; }

    //category filter dropdown
    public int? CategoryId { get; set; }
}
