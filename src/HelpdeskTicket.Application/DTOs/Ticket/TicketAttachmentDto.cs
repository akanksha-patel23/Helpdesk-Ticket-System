using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelpdeskTicket.Application.DTOs.Ticket;

public class TicketAttachmentDto
{
    public int Id { get; set; }
    public int TicketId { get; set; }
    public string AttachmentPath { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public int UploadedById { get; set; }
    public string UploadedByName { get; set; } = string.Empty;
    public DateTime UploadedDateTime { get; set; }
}