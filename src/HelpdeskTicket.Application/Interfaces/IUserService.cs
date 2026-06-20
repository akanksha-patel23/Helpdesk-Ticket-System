using HelpdeskTicket.Application.DTOs.User;

namespace HelpdeskTicket.Application.Interfaces;

// TODO: Implement in step-by-step coding phase
public interface IUserService {
    Task<List<UserDto>> GetUserListAsync(int requestedRoleId, int requestedById);
    Task<UserDto?> GetUserByIdAsync(int userId);

    //developers list
    Task<List<UserDto>> GetDevelopersAsync();
    Task<(bool Success, string Message)> UpdateUserAsync(UpdateUserRequest request);
    Task<(bool Success, string Message)> CreateUserAsync(CreateUserRequest request);
}
