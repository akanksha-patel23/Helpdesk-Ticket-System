using System.ComponentModel.DataAnnotations;

namespace HelpdeskTicket.Application.DTOs.User;

// TODO: Implement in step-by-step coding phase
public class UpdateUserRequest {
    [Required]
    public int UserId { get; set; }

    [Required(ErrorMessage = "Full name is required.")]
    [MaxLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Enter a valid email address.")]
    [MaxLength(150)]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Role is required.")]
    public int RoleId { get; set; }

    public bool IsActive { get; set; }
}
