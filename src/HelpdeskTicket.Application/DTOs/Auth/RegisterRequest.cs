using System.ComponentModel.DataAnnotations;

namespace HelpdeskTicket.Application.DTOs.Auth;

// TODO: Implement in step-by-step coding phase
public class RegisterRequest {
    [Required(ErrorMessage = "Full name is required.")]
    [MaxLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required.")]
    [MaxLength(150)]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required.")]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters.")]
    public string Password { get; set; } = string.Empty;

    // RoleId is NOT accepted from the client.
    // Always set to EndUser (3) by AccountController before sending to API.
    // Admin elevates roles later from User Management.
    public int RoleId { get; set; } = 3;
}
