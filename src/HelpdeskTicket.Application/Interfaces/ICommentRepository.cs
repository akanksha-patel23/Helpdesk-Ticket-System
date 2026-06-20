using HelpdeskTicket.Application.DTOs.Comment;

namespace HelpdeskTicket.Application.Interfaces;

// TODO: Implement in step-by-step coding phase
public interface ICommentRepository {
    Task<(bool Success, string Message)> AddCommentAsync(AddCommentRequest request);
}
