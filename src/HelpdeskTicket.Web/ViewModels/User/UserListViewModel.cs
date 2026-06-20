using HelpdeskTicket.Application.DTOs.User;

namespace HelpdeskTicket.Web.ViewModels.User;

public class UserListViewModel
{
    public List<UserDto> Users { get; set; } = [];
    public string? SearchText { get; set; }

    // Latest 10 for the quick-panel at top of page
    //public List<UserDto> RecentUsers => Users.Take(10).ToList();
    public List<UserDto> RecentUsers => Users.OrderByDescending(u => u.CreatedDateTime).Take(10).ToList();

    // Client-side filtered result — searches name, email, role
    public List<UserDto> FilteredUsers => string.IsNullOrWhiteSpace(SearchText)
        ? Users
        : Users.Where(u =>
            u.FullName.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
            u.Email.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
            u.RoleName.Contains(SearchText, StringComparison.OrdinalIgnoreCase))
          .ToList();
}
