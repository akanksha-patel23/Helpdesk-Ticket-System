using HelpdeskTicket.Application.Data;
using HelpdeskTicket.Application.DTOs.Comment;
using HelpdeskTicket.Application.DTOs.Ticket;
using HelpdeskTicket.Application.Interfaces;
using HelpdeskTicket.Core.Constants;
using HelpdeskTicket.Core.Helpers;
using HelpdeskTicket.Core.Wrappers;
using Microsoft.Extensions.Logging;

namespace HelpdeskTicket.Application.Repositories;

// TODO: Implement in step-by-step coding phase
public class TicketRepository : ITicketRepository
{
    private readonly IDbConnectionFactory _db;
    private readonly ILogger<TicketRepository> _logger;

    public TicketRepository(IDbConnectionFactory db, ILogger<TicketRepository> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<int> CreateTicketAsync(CreateTicketRequest request)
    {
        try
        {
            await using var conn = await _db.CreateConnectionAsync();
            using var cmd = SqlHelper.CreateCommand(conn, StoredProcedures.CreateTicket);

            cmd.Parameters.Add(SqlHelper.ParamNVarChar("@Title", request.Title, 200));
            cmd.Parameters.Add(SqlHelper.ParamNVarChar("@Description", request.Description, -1));
            cmd.Parameters.Add(SqlHelper.ParamInt("@CategoryId", request.CategoryId));
            cmd.Parameters.Add(SqlHelper.ParamTinyInt("@PriorityId", (byte)request.PriorityId));
            cmd.Parameters.Add(SqlHelper.ParamInt("@CreatedById", request.CreatedById));
            cmd.Parameters.Add(SqlHelper.ParamNVarChar("@AttachmentPath", request.AttachmentPath, 500));

            await using var reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                var id = SqlHelper.GetInt(reader, "Id");
                _logger.LogInformation("spCreateTicket returned Id={Id}", id);
                return id;
            }

            return 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CreateTicketAsync failed for UserId:{UserId}", request.CreatedById);
            throw;
        }
    }
    public async Task<TicketDetailDto?> GetTicketByIdAsync(int ticketId, int requestedById, int requestedRoleId)
    {
        _logger.LogInformation(
    "AddAttachmentAsync called TicketId:{Id}",
    ticketId);
        try
        {
            await using var conn = await _db.CreateConnectionAsync();
            using var cmd = SqlHelper.CreateCommand(conn, StoredProcedures.GetTicketById);
            cmd.Parameters.Add(SqlHelper.ParamInt("@Id", ticketId));
            cmd.Parameters.Add(SqlHelper.ParamInt("@RequestedById", requestedById));
            cmd.Parameters.Add(SqlHelper.ParamInt("@RequestedRoleId", requestedRoleId));

            await using var reader = await cmd.ExecuteReaderAsync();

            TicketDetailDto? ticket = null;

            // Result Set 0: ticket header
            if (await reader.ReadAsync())
            {
                ticket = new TicketDetailDto
                {
                    Id = SqlHelper.GetInt(reader, "Id"),
                    Title = SqlHelper.GetString(reader, "Title"),
                    Description = SqlHelper.GetString(reader, "Description"),
                    //AttachmentPath = SqlHelper.GetNullableString(reader, "AttachmentPath"),
                    StatusId = SqlHelper.GetInt(reader, "StatusId"),
                    StatusName = SqlHelper.GetString(reader, "StatusName"),
                    PriorityId = SqlHelper.GetInt(reader, "PriorityId"),
                    PriorityName = SqlHelper.GetString(reader, "PriorityName"),
                    CategoryId = SqlHelper.GetInt(reader, "CategoryId"),
                    CategoryName = SqlHelper.GetString(reader, "CategoryName"),
                    AssignedToId = SqlHelper.GetNullableInt(reader, "AssignedToId"),
                    AssignedToName = SqlHelper.GetNullableString(reader, "AssignedToName") ?? "Unassigned",
                    CreatedById = SqlHelper.GetInt(reader, "CreatedById"),
                    CreatedByName = SqlHelper.GetString(reader, "CreatedByName"),
                    //CreatedDateTime = SqlHelper.GetDateTime(reader, "CreatedDateTime"),
                    //UpdatedDateTime = SqlHelper.GetNullableDateTime(reader, "UpdatedDateTime")
                    CreatedDateTime = DateTimeHelper.ToIst(SqlHelper.GetDateTime(reader, "CreatedDateTime")),
                    UpdatedDateTime = DateTimeHelper.ToIst(SqlHelper.GetNullableDateTime(reader, "UpdatedDateTime")),
                };
            }

            if (ticket is null) return null;

            // Result Set 1: comments
            await reader.NextResultAsync();
            while (await reader.ReadAsync())
            {
                ticket.Comments.Add(new CommentDto
                {
                    Id = SqlHelper.GetInt(reader, "Id"),
                    Body = SqlHelper.GetString(reader, "Body"),
                    CreatedByName = SqlHelper.GetString(reader, "CreatedByName"),
                    CreatedByRole = SqlHelper.GetString(reader, "CreatedByRole"),
                    //CreatedDateTime = SqlHelper.GetDateTime(reader, "CreatedDateTime")
                    CreatedDateTime = DateTimeHelper.ToIst(SqlHelper.GetDateTime(reader, "CreatedDateTime"))
                });
            }
            // Result Set 2: attachments
            await reader.NextResultAsync();
            while (await reader.ReadAsync())
            {
                ticket.Attachments.Add(new TicketAttachmentDto
                {
                    Id = SqlHelper.GetInt(reader, "Id"),
                    TicketId = SqlHelper.GetInt(reader, "TicketId"),
                    AttachmentPath = SqlHelper.GetString(reader, "AttachmentPath"),
                    FileName = SqlHelper.GetString(reader, "FileName"),
                    UploadedById = SqlHelper.GetInt(reader, "UploadedById"),
                    UploadedByName = SqlHelper.GetString(reader, "UploadedByName"),
                    //UploadedDateTime = SqlHelper.GetDateTime(reader, "UploadedDateTime")
                    //UploadedDateTime = DateTimeHelper.ToIst(SqlHelper.GetDateTime(reader, "UploadedDateTime"))
                    UploadedDateTime = DateTimeHelper.ToIst(reader.GetDateTime(reader.GetOrdinal("UploadedDateTime")))
                });
            }
            return ticket;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetTicketByIdAsync failed TicketId:{Id}", ticketId);
            throw;
        }
    }

    public async Task<(bool Success, string Message, TicketEmailInfoDto? EmailInfo)>   UpdateTicketAsync(UpdateTicketRequest request)
    {
        try
        {
            await using var conn = await _db.CreateConnectionAsync();
            using var cmd = SqlHelper.CreateCommand(conn, StoredProcedures.UpdateTicket);
            cmd.Parameters.Add(SqlHelper.ParamInt("@Id", request.TicketId));
            cmd.Parameters.Add(SqlHelper.ParamNVarChar("@Title", request.Title, 200));
            cmd.Parameters.Add(SqlHelper.ParamNVarChar("@Description", request.Description, -1));
            cmd.Parameters.Add(SqlHelper.ParamInt("@StatusId", request.StatusId));
            cmd.Parameters.Add(SqlHelper.ParamInt("@PriorityId", request.PriorityId));
            cmd.Parameters.Add(SqlHelper.ParamInt("@AssignedToId", request.AssignedToId));
            cmd.Parameters.Add(SqlHelper.ParamInt("@UpdatedById", request.UpdatedById));
            //cmd.Parameters.Add(SqlHelper.ParamNVarChar("@AttachmentPath", request.AttachmentPath, 500));

            await using var reader = await cmd.ExecuteReaderAsync();

            bool success = false;
            string message = "No response from update procedure.";

            if (await reader.ReadAsync())
            {
                //var success = SqlHelper.GetInt(reader, "Success") == 1;
                //var message = SqlHelper.GetString(reader, "Message");
                //return (success, message);
                success = SqlHelper.GetInt(reader, "Success") == 1;
                message = SqlHelper.GetString(reader, "Message");
            }

            // NEW: read second result set for email info — only if update succeeded
            TicketEmailInfoDto? emailInfo = null;
            try
            {
                if (success && await reader.NextResultAsync() && await reader.ReadAsync())
                {
                    emailInfo = new TicketEmailInfoDto
                    {
                        TicketId = SqlHelper.GetInt(reader, "TicketId"),
                        Title = SqlHelper.GetString(reader, "Title"),
                        Description = SqlHelper.GetString(reader, "Description"),
                        StatusId = SqlHelper.GetInt(reader, "StatusId"),
                        StatusName = SqlHelper.GetString(reader, "StatusName"),
                        CreatedById = SqlHelper.GetInt(reader, "CreatedById"),
                        CreatedByEmail = SqlHelper.GetString(reader, "CreatedByEmail"),
                        CreatedByName = SqlHelper.GetString(reader, "CreatedByName"),
                        AssignedToId = SqlHelper.GetNullableInt(reader, "AssignedToId"),
                        AssignedToEmail = SqlHelper.GetNullableString(reader, "AssignedToEmail"),
                        AssignedToName = SqlHelper.GetNullableString(reader, "AssignedToName"),
                        AssignedById = SqlHelper.GetNullableInt(reader, "AssignedById"),
                        AssignedByEmail = SqlHelper.GetNullableString(reader, "AssignedByEmail"),
                        AssignedByName = SqlHelper.GetNullableString(reader, "AssignedByName"),
                        OldAssignedToId = SqlHelper.GetNullableInt(reader, "OldAssignedToId")
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed reading email info result set for TicketId:{Id}", request.TicketId);
                // Don't fail the whole update just because email-info reading failed
            }
            return (success, message, emailInfo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "UpdateTicketAsync failed TicketId:{Id}", request.TicketId);
            throw;
        }
    }
    public async Task<List<(int Id, string FullName, string Email)>> GetActiveAdminEmailsAsync()
    {
        var admins = new List<(int Id, string FullName, string Email)>();
        try
        {
            await using var conn = await _db.CreateConnectionAsync();
            using var cmd = SqlHelper.CreateCommand(conn, StoredProcedures.GetActiveAdminEmails);
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                admins.Add((
                    SqlHelper.GetInt(reader, "Id"),
                    SqlHelper.GetString(reader, "FullName"),
                    SqlHelper.GetString(reader, "Email")
                ));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetActiveAdminEmailsAsync failed");
            throw;
        }
        return admins;
    }

    public async Task<(bool Success, string Message)> AddAttachmentAsync(
    int ticketId, string path, string fileName, int uploadedById)
    {
        _logger.LogInformation(
    "AddAttachmentAsync called TicketId:{Id}",
    ticketId);
        try
        {
            await using var conn = await _db.CreateConnectionAsync();
            using var cmd = SqlHelper.CreateCommand(conn, StoredProcedures.AddTicketAttachment);
            cmd.Parameters.Add(SqlHelper.ParamInt("@TicketId", ticketId));
            cmd.Parameters.Add(SqlHelper.ParamNVarChar("@AttachmentPath", path, 500));
            cmd.Parameters.Add(SqlHelper.ParamNVarChar("@FileName", fileName, 255));
            cmd.Parameters.Add(SqlHelper.ParamInt("@UploadedById", uploadedById));

            await using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
                return (SqlHelper.GetInt(reader, "Id") > 0,
                        SqlHelper.GetString(reader, "Message"));
            return (false, "Failed to save attachment.");
        }
        catch (Exception ex) { _logger.LogError(ex, "AddAttachmentAsync failed"); throw; }
    }

    public async Task<(bool Success, string Message)> DeleteAttachmentAsync(
        int attachmentId, int requestedById)
    {
        try
        {
            await using var conn = await _db.CreateConnectionAsync();
            using var cmd = SqlHelper.CreateCommand(conn, StoredProcedures.DeleteTicketAttachment);
            cmd.Parameters.Add(SqlHelper.ParamInt("@Id", attachmentId));
            cmd.Parameters.Add(SqlHelper.ParamInt("@RequestedById", requestedById));

            await using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
                return (SqlHelper.GetInt(reader, "Success") == 1,
                        SqlHelper.GetString(reader, "Message"));
            return (false, "Failed to delete attachment.");
        }
        catch (Exception ex) { _logger.LogError(ex, "DeleteAttachmentAsync failed"); throw; }
    }
    public async Task<PagedResponse<TicketListDto>> GetTicketListAsync(TicketFilterRequest filter)
    {
        var tickets = new List<TicketListDto>();
        int totalCount = 0;

        try
        {
            await using var conn = await _db.CreateConnectionAsync();
            using var cmd = SqlHelper.CreateCommand(conn, StoredProcedures.GetTicketList);

            cmd.Parameters.Add(SqlHelper.ParamInt("@RequestedById", filter.RequestedById));
            cmd.Parameters.Add(SqlHelper.ParamInt("@RequestedRoleId", filter.RequestedRoleId));
            cmd.Parameters.Add(SqlHelper.ParamNVarChar("@SearchText", filter.SearchText, 200));
            cmd.Parameters.Add(SqlHelper.ParamInt("@StatusId", filter.StatusId));
            cmd.Parameters.Add(SqlHelper.ParamInt("@PriorityId", filter.PriorityId));
            cmd.Parameters.Add(SqlHelper.ParamInt("@PageNumber", filter.PageNumber));
            cmd.Parameters.Add(SqlHelper.ParamInt("@PageSize", filter.PageSize));
            cmd.Parameters.Add(SqlHelper.ParamInt("@CategoryId", filter.CategoryId));

            await using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                // TotalCount is returned inline on every row by the SP (COUNT(1) OVER())
                totalCount = SqlHelper.GetInt(reader, "TotalCount");

                tickets.Add(new TicketListDto
                {
                    Id = SqlHelper.GetInt(reader, "Id"),
                    Title = SqlHelper.GetString(reader, "Title"),
                    StatusName = SqlHelper.GetString(reader, "StatusName"),
                    PriorityName = SqlHelper.GetString(reader, "PriorityName"),
                    CategoryName = SqlHelper.GetString(reader, "CategoryName"),
                    CreatedByName = SqlHelper.GetString(reader, "CreatedByName"),
                    AssignedToName = SqlHelper.GetNullableString(reader, "AssignedToName") ?? "Unassigned",
                    //CreatedDateTime = SqlHelper.GetDateTime(reader, "CreatedDateTime"),
                    //UpdatedDateTime = SqlHelper.GetNullableDateTime(reader, "UpdatedDateTime")
                    CreatedDateTime = DateTimeHelper.ToIst(SqlHelper.GetDateTime(reader, "CreatedDateTime")),
                    UpdatedDateTime = DateTimeHelper.ToIst(SqlHelper.GetNullableDateTime(reader, "UpdatedDateTime")),
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetTicketListAsync failed UserId:{Id} RoleId:{Role}",
                filter.RequestedById, filter.RequestedRoleId);
            throw;
        }

        return PagedResponse<TicketListDto>.Ok(tickets, totalCount, filter.PageNumber, filter.PageSize);
    }

    public async Task<List<LookupDto>> GetCategoriesAsync()
    {
        var list = new List<LookupDto>();
        try
        {
            await using var conn = await _db.CreateConnectionAsync();
            using var cmd = SqlHelper.CreateCommand(conn, StoredProcedures.GetAllCategory);
            await using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
                list.Add(new LookupDto
                {
                    Id = SqlHelper.GetInt(reader, "Id"),
                    Name = SqlHelper.GetString(reader, "Name")
                });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetCategoriesAsync failed.");
            throw;
        }
        return list;
    }

    public async Task<List<LookupDto>> GetPrioritiesAsync()
    {
        var list = new List<LookupDto>();
        try
        {
            await using var conn = await _db.CreateConnectionAsync();
            using var cmd = SqlHelper.CreateCommand(conn, StoredProcedures.GetAllPriority);
            await using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
                list.Add(new LookupDto
                {
                    Id = SqlHelper.GetInt(reader, "Id"),
                    Name = SqlHelper.GetString(reader, "Name")
                });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetPrioritiesAsync failed.");
            throw;
        }
        return list;
    }

    public async Task<TicketDropdownsDto> GetDropdownsAsync()
    {
        var categories = await GetCategoriesAsync();
        var priorities = await GetPrioritiesAsync();
        return new TicketDropdownsDto { Categories = categories, Priorities = priorities };
    }
}