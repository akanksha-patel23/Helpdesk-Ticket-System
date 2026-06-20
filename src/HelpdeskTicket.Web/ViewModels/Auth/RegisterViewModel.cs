using System.ComponentModel.DataAnnotations;

namespace HelpdeskTicket.Web.ViewModels.Auth;
    public class RegisterViewModel
    {
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

        [Required(ErrorMessage = "Password is required.")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters.")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please confirm your password.")]
    [DataType(DataType.Password)]
    [Display(Name = "Confirm Password")]
    [Compare(nameof(Password), ErrorMessage = "Passwords do not match.")]
    public string ConfirmPassword { get; set; } = string.Empty;

    //[Required(ErrorMessage = "Role is required.")]
    //    [Display(Name = "Role")]
    //    public int RoleId { get; set; }
    }
