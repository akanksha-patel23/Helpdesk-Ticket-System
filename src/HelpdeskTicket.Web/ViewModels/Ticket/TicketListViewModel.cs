using HelpdeskTicket.Application.DTOs.Ticket;
using HelpdeskTicket.Core.Wrappers;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HelpdeskTicket.Web.ViewModels.Ticket;

public class TicketListViewModel
{
    // Paged result from API
    public PagedResponse<TicketListDto> PagedResult { get; set; } = new();

    // Current filter values — kept in form to retain state across pages
    public string? SearchText { get; set; }
    public int? StatusId { get; set; }
    public int? PriorityId { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;

    // Dropdown options for filter bar
    public List<SelectListItem> Statuses { get; set; } = [];
    public List<SelectListItem> Priorities { get; set; } = [];

    public int? CategoryId { get; set; }
    public List<SelectListItem> Categories { get; set; } = [];
}
