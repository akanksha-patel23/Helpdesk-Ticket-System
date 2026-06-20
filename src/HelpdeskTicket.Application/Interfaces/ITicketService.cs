using HelpdeskTicket.Application.DTOs.Ticket;
using HelpdeskTicket.Core.Wrappers;

namespace HelpdeskTicket.Application.Interfaces;

// TODO: Implement in step-by-step coding phase
public interface ITicketService
{
    Task<(bool Success, string Message, int TicketId)> CreateTicketAsync(CreateTicketRequest request);
    Task<TicketDetailDto?> GetTicketByIdAsync(int ticketId, int requestedById, int requestedRoleId);
    Task<(bool Success, string Message)> UpdateTicketAsync(UpdateTicketRequest request);
    Task<PagedResponse<TicketListDto>> GetTicketListAsync(TicketFilterRequest filter);
    Task<TicketDropdownsDto> GetDropdownsAsync();

    // Attachments
    Task<(bool Success, string Message)> AddAttachmentAsync(int ticketId, string path, string fileName, int uploadedById);
    Task<(bool Success, string Message)> DeleteAttachmentAsync(int attachmentId, int requestedById);
}
