using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelpdeskTicket.Application.Interfaces;

public interface IEmailService
{
    Task SendResetLinkAsync(string toEmail, string toName, string resetLink);
    Task SendTicketCreatedAsync(string toEmail, string toName, int ticketId, string title, string description);
    Task SendTicketAssignedAsync(string toEmail, string toName, int ticketId, string title, string description, string assignedToName, bool isOwner);
    Task SendStatusChangedAsync(string toEmail, string toName, int ticketId, string title, string description, string newStatus, bool richContent, bool isOwner);
    Task SendTicketClosedAsync(string toEmail, string toName, int ticketId, string title, string description, bool isOwner);
}
