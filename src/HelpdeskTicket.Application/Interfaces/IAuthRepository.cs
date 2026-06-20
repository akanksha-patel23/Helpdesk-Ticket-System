using HelpdeskTicket.Application.Models;

namespace HelpdeskTicket.Application.Interfaces;

// TODO: Implement in step-by-step coding phase
public interface IAuthRepository {
    Task<User?> GetUserByEmailAsync(string email);
    Task<int> RegisterUserAsync(string fullName, string email, string passwordHash, int roleId);
    Task<bool> UpdatePasswordAsync(string email, string passwordHash);
    Task<bool> SavePasswordResetTokenAsync(string email, Guid token, DateTime expiry);
    Task<(bool Success, string Message)> ResetPasswordByTokenAsync(Guid token, string passwordHash);
    Task<bool> ValidateResetTokenAsync(Guid token);
}
