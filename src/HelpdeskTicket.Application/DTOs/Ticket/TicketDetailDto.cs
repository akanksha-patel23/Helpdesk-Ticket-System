using HelpdeskTicket.Application.DTOs.Comment;

namespace HelpdeskTicket.Application.DTOs.Ticket;

// TODO: Implement in step-by-step coding phase
public class TicketDetailDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    //public string? AttachmentPath { get; set; }

    public int StatusId { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public int PriorityId { get; set; }
    public string PriorityName { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;

    public int? AssignedToId { get; set; }
    public string AssignedToName { get; set; } = string.Empty;
    public int CreatedById { get; set; }
    public string CreatedByName { get; set; } = string.Empty;

    public DateTime CreatedDateTime { get; set; }
    public DateTime? UpdatedDateTime { get; set; }

    public List<CommentDto> Comments { get; set; } = [];

    // Multiple attachments — replaces single AttachmentPath
    public List<TicketAttachmentDto> Attachments { get; set; } = [];
}
