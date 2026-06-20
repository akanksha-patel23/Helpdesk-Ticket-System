using System.ComponentModel.DataAnnotations;

namespace HelpdeskTicket.Web.ViewModels.User;

public class ProfileViewModel
{
    public int UserId { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public DateTime CreatedDateTime { get; set; }

    [Required(ErrorMessage = "Full name is required.")]
    [MaxLength(100)]
    [Display(Name = "Full Name")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required.")]
    //[EmailAddress(ErrorMessage = "Enter a valid email address.")]
    [MaxLength(150)]
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
    public string Email { get; set; } = string.Empty;
}
