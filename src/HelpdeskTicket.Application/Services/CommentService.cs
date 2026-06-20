using HelpdeskTicket.Application.DTOs.Comment;
using HelpdeskTicket.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace HelpdeskTicket.Application.Services;

// TODO: Implement in step-by-step coding phase
public class CommentService : ICommentService
{
    private readonly ICommentRepository _repo;
    private readonly ILogger<CommentService> _logger;

    public CommentService(ICommentRepository repo, ILogger<CommentService> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    public async Task<(bool Success, string Message)> AddCommentAsync(AddCommentRequest request)
    {
        _logger.LogInformation("AddComment TicketId:{Id} UserId:{UserId}", request.TicketId, request.CreatedById);
        return await _repo.AddCommentAsync(request);
    }
}
