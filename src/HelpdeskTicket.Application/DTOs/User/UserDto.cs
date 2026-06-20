namespace HelpdeskTicket.Application.DTOs.User;

// TODO: Implement in step-by-step coding phase
public class UserDto {
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedDateTime { get; set; }
}
