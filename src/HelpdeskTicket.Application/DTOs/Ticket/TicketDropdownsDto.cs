using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelpdeskTicket.Application.DTOs.Ticket;
// Carries all dropdown data needed to render the Create Ticket form
public class TicketDropdownsDto
{
    public List<LookupDto> Categories { get; set; } = [];
    public List<LookupDto> Priorities { get; set; } = [];
}
