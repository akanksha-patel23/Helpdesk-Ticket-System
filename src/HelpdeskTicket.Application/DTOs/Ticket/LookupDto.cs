using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelpdeskTicket.Application.DTOs.Ticket;
// Generic Id+Name pair used for Category, Priority, Status dropdowns
public class LookupDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
