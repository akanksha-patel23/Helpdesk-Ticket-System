using System.ComponentModel.DataAnnotations;

namespace HelpdeskTicket.Web.ViewModels.Auth;

    public class LoginViewModel
    {
    [Required(ErrorMessage = "Email is required.")]
    //[EmailAddress(ErrorMessage = "Enter a valid email address.")]

    //[RegularExpression(
    //@"^[a-zA-Z0-9._%+-]+@((gmail|metasyssoftware)\.com|ksil\.org\.in)$",
    //ErrorMessage = "Email must be a valid address ending in @gmail.com, @metasyssoftware.com, or @ksil.org.in.")]

    [RegularExpression(
    @"^[a-zA-Z0-9._%+-]+@(gmail\.com|metasyssoftware\.com|ksil\.org\.in|maildrop\.cc)$",
    ErrorMessage = "Email must be from an approved domain.")]

    [Display(Name = "Email Address")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required.")]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters.")]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "Remember Me")]
    public bool RememberMe { get; set; }
}
