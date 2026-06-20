using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelpdeskTicket.Application.DTOs.Auth;

public class ResetPasswordRequest
{
    //[Required]
    //public string Email { get; set; } = string.Empty;
    [Required]
    public string Token { get; set; } = string.Empty;   // GUID string

    [Required(ErrorMessage = "Password is required.")]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters.")]
    public string NewPassword { get; set; } = string.Empty;
}
