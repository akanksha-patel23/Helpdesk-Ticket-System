using HelpdeskTicket.Application.DTOs.Ticket;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HelpdeskTicket.Web.ViewModels.Ticket;

public class TicketDetailViewModel
{
    // Full ticket data from API
    public TicketDetailDto Ticket { get; set; } = new();

    // Dropdowns for update panel (Admin/Developer only)
    public List<SelectListItem> Statuses { get; set; } = [];
    public List<SelectListItem> Priorities { get; set; } = [];
    public List<SelectListItem> Developers { get; set; } = [];

    // Comment input field — bound on POST
    public string CommentBody { get; set; } = string.Empty;
}
