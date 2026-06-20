using System.ComponentModel.DataAnnotations;

namespace HelpdeskTicket.Application.DTOs.Comment;

// TODO: Implement in step-by-step coding phase
public class AddCommentRequest {
    [Required]
    public int TicketId { get; set; }

    [Required(ErrorMessage = "Comment cannot be empty.")]
    [MaxLength(1000, ErrorMessage = "Comment cannot exceed 1000 characters.")]
    public string Body { get; set; } = string.Empty;

    // Set server-side from session — never from client
    public int CreatedById { get; set; }
}
