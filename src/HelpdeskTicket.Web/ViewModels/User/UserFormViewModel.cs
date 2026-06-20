using HelpdeskTicket.Application.DTOs.User;
using System.ComponentModel.DataAnnotations;

namespace HelpdeskTicket.Web.ViewModels.User;

public class UserFormViewModel
{
    public int UserId { get; set; }  // 0 = create, >0 = edit

    [Required(ErrorMessage = "Full name is required.")]
    [MaxLength(100)]
    [Display(Name = "Full Name")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required.")]
    //[EmailAddress(ErrorMessage = "Enter a valid email address.")]
    [Display(Name = "Email")]
    //[RegularExpression(
    //@"^[a-zA-Z0-9._%+-]+@(gmail|metasyssoftware)\.com$",
    //ErrorMessage = "Email must be a valid address ending in .com (e.g. name@gmail.com).")]

    //[RegularExpression(
    //@"^[a-zA-Z0-9._%+-]+@((gmail|metasyssoftware)\.com|ksil\.org\.in)$",
    //ErrorMessage = "Email must be a valid address ending in @gmail.com, @metasyssoftware.com, or @ksil.org.in.")]

    [RegularExpression(
    @"^[a-zA-Z0-9._%+-]+@(gmail\.com|metasyssoftware\.com|ksil\.org\.in|maildrop\.cc)$",
    ErrorMessage = "Email must be from an approved domain.")]

    [MaxLength(150)]
    public string Email { get; set; } = string.Empty;

    [MinLength(6, ErrorMessage = "Password must be at least 6 characters.")]
    [Display(Name = "Password")]
    public string? Password { get; set; }

    [Required(ErrorMessage = "Role is required.")]
    [Display(Name = "Role")]
    public int RoleId { get; set; }

    [Display(Name = "Active")]
    public bool IsActive { get; set; } = true;

    public bool IsEdit => UserId > 0;

    public static UserFormViewModel FromDto(UserDto dto) => new()
    {
        UserId = dto.Id,
        FullName = dto.FullName,
        Email = dto.Email,
        RoleId = dto.RoleId,
        IsActive = dto.IsActive
    };
}
