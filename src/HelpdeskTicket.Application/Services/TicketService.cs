using HelpdeskTicket.Application.DTOs.Ticket;
using HelpdeskTicket.Application.Interfaces;
using HelpdeskTicket.Core.Constants;
using HelpdeskTicket.Core.Wrappers;
using Microsoft.Extensions.Logging;

namespace HelpdeskTicket.Application.Services;

// TODO: Implement in step-by-step coding phase
public class TicketService : ITicketService
{
    private readonly ITicketRepository _repo;
    private readonly ILogger<TicketService> _logger;
    private readonly IEmailService _emailService;

    public TicketService(ITicketRepository repo, IEmailService emailService,ILogger<TicketService> logger)
    {
        _repo = repo;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<(bool Success, string Message, int TicketId)> CreateTicketAsync(CreateTicketRequest request)
    {
        var newId = await _repo.CreateTicketAsync(request);
        _logger.LogInformation(    "CreateTicket AttachmentPath = {Path}",    request.AttachmentPath);
        if (newId <= 0)
        {
            _logger.LogWarning("Ticket creation returned Id={Id} for UserId:{UserId}", newId, request.CreatedById);
            return (false, "Failed to create ticket. Please try again.", 0);
        }
        // NEW
        if (!string.IsNullOrWhiteSpace(request.AttachmentPath))
        {
            _logger.LogInformation("Calling AddAttachmentAsync TicketId:{Id} Path:{Path}",newId,request.AttachmentPath);
            await _repo.AddAttachmentAsync(
                newId,
                request.AttachmentPath,
                Path.GetFileName(request.AttachmentPath),
                request.CreatedById);
        }

        // NEW: email notify all active admins
        try
        {
            var admins = await _repo.GetActiveAdminEmailsAsync();
            foreach (var admin in admins)
            {
                await _emailService.SendTicketCreatedAsync(
                    admin.Email, admin.FullName, newId, request.Title, request.Description);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed sending admin notification emails for TicketId:{Id}", newId);
        }

        _logger.LogInformation("Ticket #{Id} created by UserId:{UserId}", newId, request.CreatedById);
        return (true, "Ticket created successfully.", newId);
    }
    public async Task<TicketDetailDto?> GetTicketByIdAsync(int ticketId, int requestedById, int requestedRoleId)
    {
        _logger.LogInformation("GetTicketById TicketId:{Id} UserId:{UserId}", ticketId, requestedById);
        return await _repo.GetTicketByIdAsync(ticketId, requestedById, requestedRoleId);
    }

    public async Task<(bool Success, string Message)> UpdateTicketAsync(UpdateTicketRequest request)
    {
        if (request.UpdatedByRoleId == Roles.DeveloperId)
        {
            if (request.StatusId == 1 || request.StatusId == 4)
            {
                return (false, "Developers cannot change ticket status to Open, Assigned or Closed.");
            }
        }
        _logger.LogInformation("UpdateTicket TicketId:{Id} UpdatedBy:{UserId}", request.TicketId, request.UpdatedById);
        //return await _repo.UpdateTicketAsync(request);

        var (success, message, info) = await _repo.UpdateTicketAsync(request);

        // Email sending hook will go here in the next step 
        if (success && info is not null)
        {
            await SendStatusNotificationsAsync(request, info);
        }
        return (success, message);
    }

    private async Task SendStatusNotificationsAsync(UpdateTicketRequest request, TicketEmailInfoDto info)
    {
        try
        {
            // Assigned (StatusId == 2) — Developer + EndUser notified
            if (request.AssignedToId.HasValue 
                && info.AssignedToId == request.AssignedToId
                 && info.OldAssignedToId != request.AssignedToId
                && !string.IsNullOrWhiteSpace(info.AssignedToEmail))
            {
                await _emailService.SendTicketAssignedAsync(
                    info.AssignedToEmail!, info.AssignedToName ?? "Developer",
                    info.TicketId, info.Title, info.Description, info.AssignedToName ?? "Developer", isOwner: false);

                await _emailService.SendTicketAssignedAsync(
                    info.CreatedByEmail, info.CreatedByName,
                    info.TicketId, info.Title, info.Description, info.AssignedToName ?? "Developer", isOwner: true);

            }

            // In Progress (StatusId == 3) — EndUser only, lighter content
            if (request.StatusId == 3)
            {
                await _emailService.SendStatusChangedAsync(
                    info.CreatedByEmail, info.CreatedByName,
                    info.TicketId, info.Title, info.Description, "In Progress", richContent: false, isOwner: true);
            }

            // Resolved (StatusId == 5) — EndUser + the other party (Admin or Developer)
            if (request.StatusId == 5)
            {
                await _emailService.SendStatusChangedAsync(
                    info.CreatedByEmail, info.CreatedByName,
                    info.TicketId, info.Title, info.Description, "Resolved", richContent: true, isOwner: true);

                if (request.UpdatedByRoleId == Roles.DeveloperId
                    && !string.IsNullOrWhiteSpace(info.AssignedByEmail))
                {
                    // Developer resolved ? notify the Admin who assigned it
                    await _emailService.SendStatusChangedAsync(
                    info.AssignedByEmail!, info.AssignedByName ?? "Admin",
                    info.TicketId, info.Title, info.Description, "Resolved", richContent: true, isOwner: false);
                }
                else if (request.UpdatedByRoleId == Roles.AdminId
                    && !string.IsNullOrWhiteSpace(info.AssignedToEmail))
                {
                    // Admin resolved ? notify the Developer
                    await _emailService.SendStatusChangedAsync(
                    info.AssignedToEmail!, info.AssignedToName ?? "Developer",
                    info.TicketId, info.Title, info.Description, "Resolved", richContent: true, isOwner: false);
                }
            }

            // Closed (StatusId == 4) — Developer + EndUser, Admin always performs this
            if (request.StatusId == 4)
            {
                await _emailService.SendTicketClosedAsync(
                    info.CreatedByEmail, info.CreatedByName,
                    info.TicketId, info.Title, info.Description, isOwner: true);

                if (!string.IsNullOrWhiteSpace(info.AssignedToEmail))
                {
                    await _emailService.SendTicketClosedAsync(
                    info.AssignedToEmail!, info.AssignedToName ?? "Developer",
                    info.TicketId, info.Title, info.Description, isOwner: false);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed sending notification emails for TicketId:{Id}", request.TicketId);
            // Never fail the ticket update because of email issues
        }
    }
    public async Task<PagedResponse<TicketListDto>> GetTicketListAsync(TicketFilterRequest filter)
    {
        _logger.LogInformation("GetTicketList UserId:{Id} RoleId:{Role} Page:{Page}",
            filter.RequestedById, filter.RequestedRoleId, filter.PageNumber);
        return await _repo.GetTicketListAsync(filter);
    }

    public async Task<TicketDropdownsDto> GetDropdownsAsync()
        => await _repo.GetDropdownsAsync();

    public async Task<(bool Success, string Message)> AddAttachmentAsync(
    int ticketId, string path, string fileName, int uploadedById)
    => await _repo.AddAttachmentAsync(ticketId, path, fileName, uploadedById);

    public async Task<(bool Success, string Message)> DeleteAttachmentAsync(
        int attachmentId, int requestedById)
        => await _repo.DeleteAttachmentAsync(attachmentId, requestedById);
}
