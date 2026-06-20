using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelpdeskTicket.Application.DTOs.Auth;

public class ForgotPasswordApiRequest
{
    public string Email { get; set; } = string.Empty;
}
