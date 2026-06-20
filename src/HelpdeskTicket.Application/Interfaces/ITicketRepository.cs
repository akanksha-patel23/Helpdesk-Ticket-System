using HelpdeskTicket.Application.DTOs.Ticket;
using HelpdeskTicket.Core.Wrappers;

namespace HelpdeskTicket.Application.Interfaces;

// TODO: Implement in step-by-step coding phase
public interface ITicketRepository
{
    Task<int> CreateTicketAsync(CreateTicketRequest request);
    Task<TicketDetailDto?> GetTicketByIdAsync(int ticketId, int requestedById, int requestedRoleId);
    Task<(bool Success, string Message, TicketEmailInfoDto? EmailInfo)> UpdateTicketAsync(UpdateTicketRequest request);
    Task<PagedResponse<TicketListDto>> GetTicketListAsync(TicketFilterRequest filter);
    Task<List<LookupDto>> GetCategoriesAsync();
    Task<List<LookupDto>> GetPrioritiesAsync();
    Task<TicketDropdownsDto> GetDropdownsAsync();

    // Attachments
    Task<(bool Success, string Message)> AddAttachmentAsync(int ticketId, string path, string fileName, int uploadedById);
    Task<(bool Success, string Message)> DeleteAttachmentAsync(int attachmentId, int requestedById);

    Task<List<(int Id, string FullName, string Email)>> GetActiveAdminEmailsAsync();

}
