using System.ComponentModel.DataAnnotations;

namespace HelpdeskTicket.Web.ViewModels.Auth;

public class ForgotPasswordViewModel
{
    [Required(ErrorMessage = "Email is required.")]
    //[EmailAddress(ErrorMessage = "Enter a valid email address.")]
    //[RegularExpression(
    //    @"^[a-zA-Z0-9._%+-]+@(gmail|metasyssoftware)\.com$",
    //    ErrorMessage = "Email must be a valid address ending in .com (e.g. name@gmail.com).")]

    //[RegularExpression(
    //@"^[a-zA-Z0-9._%+-]+@((gmail|metasyssoftware)\.com|ksil\.org\.in)$",
    //ErrorMessage = "Email must be a valid address ending in @gmail.com, @metasyssoftware.com, or @ksil.org.in.")]

    [RegularExpression(
    @"^[a-zA-Z0-9._%+-]+@(gmail\.com|metasyssoftware\.com|ksil\.org\.in|maildrop\.cc)$",
    ErrorMessage = "Email must be from an approved domain.")]

    [Display(Name = "Email Address")]
    public string Email { get; set; } = string.Empty;
}