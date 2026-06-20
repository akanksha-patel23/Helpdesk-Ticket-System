using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelpdeskTicket.Application.DTOs.Ticket;

public class TicketEmailInfoDto
{
    public int TicketId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int StatusId { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public int CreatedById { get; set; }
    public string CreatedByEmail { get; set; } = string.Empty;
    public string CreatedByName { get; set; } = string.Empty;
    public int? AssignedToId { get; set; }
    public string? AssignedToEmail { get; set; }
    public string? AssignedToName { get; set; }
    public int? AssignedById { get; set; }
    public string? AssignedByEmail { get; set; }
    public string? AssignedByName { get; set; }
    public int? OldAssignedToId { get; set; }
}
