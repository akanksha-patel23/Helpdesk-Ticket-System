using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace HelpdeskTicket.Web.ViewModels.Ticket;

public class CreateTicketViewModel
{
    [Required(ErrorMessage = "Title is required.")]
    [MaxLength(200, ErrorMessage = "Title cannot exceed 200 characters.")]
    [Display(Name = "Title")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Description is required.")]
    [Display(Name = "Description")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please select a category.")]
    [Range(1, int.MaxValue, ErrorMessage = "Please select a category.")]
    [Display(Name = "Category")]
    public int CategoryId { get; set; }

    [Required(ErrorMessage = "Please select a priority.")]
    [Range(1, int.MaxValue, ErrorMessage = "Please select a priority.")]
    [Display(Name = "Priority")]
    public int PriorityId { get; set; }

    [Display(Name = "Attachment")]
    public IFormFile? Attachment { get; set; }

    // Populated before view render — never submitted by form
    public List<SelectListItem> Categories { get; set; } = [];
    public List<SelectListItem> Priorities { get; set; } = [];
}
