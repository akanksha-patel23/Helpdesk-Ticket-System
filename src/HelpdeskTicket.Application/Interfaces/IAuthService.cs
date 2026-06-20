using HelpdeskTicket.Application.DTOs.Auth;

namespace HelpdeskTicket.Application.Interfaces;

// TODO: Implement in step-by-step coding phase
public interface IAuthService {
    Task<LoginResponse?> LoginAsync(LoginRequest request);
    Task<(bool Success, string Message)> RegisterAsync(RegisterRequest request);
    Task<(bool Success, string Message)> GeneratePasswordResetTokenAsync(string email);
    Task<(bool Success, string Message)> ResetPasswordByTokenAsync(Guid token, string newPassword);
    //Task<(bool Success, string Message)> ResetPasswordAsync(ResetPasswordRequest request);
    Task<bool> ValidateResetTokenAsync(Guid token);
}
