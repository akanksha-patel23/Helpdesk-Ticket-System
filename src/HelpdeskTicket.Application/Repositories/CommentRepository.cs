using HelpdeskTicket.Application.Data;
using HelpdeskTicket.Application.DTOs.Comment;
using HelpdeskTicket.Application.Interfaces;
using HelpdeskTicket.Core.Constants;
using Microsoft.Extensions.Logging;

namespace HelpdeskTicket.Application.Repositories;

// TODO: Implement in step-by-step coding phase
public class CommentRepository : ICommentRepository
{
    private readonly IDbConnectionFactory _db;
    private readonly ILogger<CommentRepository> _logger;

    public CommentRepository(IDbConnectionFactory db, ILogger<CommentRepository> logger)
    {
        _db = db;
        _logger = logger;
    }
    public async Task<(bool Success, string Message)> AddCommentAsync(AddCommentRequest request)
    {
        try
        {
            await using var conn = await _db.CreateConnectionAsync();
            using var cmd = SqlHelper.CreateCommand(conn, StoredProcedures.AddComment);
            cmd.Parameters.Add(SqlHelper.ParamInt("@TicketId", request.TicketId));
            cmd.Parameters.Add(SqlHelper.ParamNVarChar("@Body", request.Body, 1000));
            cmd.Parameters.Add(SqlHelper.ParamInt("@CreatedById", request.CreatedById));

            await using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                var id = SqlHelper.GetInt(reader, "Id");
                var message = SqlHelper.GetString(reader, "Message");
                return (id > 0, message);
            }
            return (false, "No response from comment procedure.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AddCommentAsync failed TicketId:{Id}", request.TicketId);
            throw;
        }
    }
}
