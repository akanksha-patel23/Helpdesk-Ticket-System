using System.ComponentModel.DataAnnotations;

namespace HelpdeskTicket.Application.DTOs.Ticket;

// TODO: Implement in step-by-step coding phase
public class CreateTicketRequest {
    [Required(ErrorMessage = "Title is required.")]
    [MaxLength(200, ErrorMessage = "Title cannot exceed 200 characters.")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Description is required.")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Category is required.")]
    [Range(1, int.MaxValue, ErrorMessage = "Please select a category.")]
    public int CategoryId { get; set; }

    [Required(ErrorMessage = "Priority is required.")]
    [Range(1, int.MaxValue, ErrorMessage = "Please select a priority.")]
    public int PriorityId { get; set; }

    // Set server-side from session — never trusted from client
    public int CreatedById { get; set; }

    // Populated by API after saving file to wwwroot/uploads
    public string? AttachmentPath { get; set; }
}
