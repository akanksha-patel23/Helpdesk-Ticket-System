namespace HelpdeskTicket.Application.DTOs.Ticket;

// TODO: Implement in step-by-step coding phase
public class UpdateTicketRequest
{
    public int TicketId { get; set; }
    public string? Title { get; set; }   // null = no change
    public string? Description { get; set; }   // null = no change
    public int? StatusId { get; set; }
    public int? PriorityId { get; set; }
    public int? AssignedToId { get; set; }
    //public string? AttachmentPath { get; set; }

    // Set server-side from session
    public int UpdatedById { get; set; }
    public int UpdatedByRoleId { get; set; }
}
