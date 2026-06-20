using HelpdeskTicket.Application.DTOs.Category;
using System.ComponentModel.DataAnnotations;

namespace HelpdeskTicket.Web.ViewModels.Category;

public class CategoryViewModel
{
    // Full list for the grid
    public List<CategoryDto> Categories { get; set; } = [];

    // Form fields — used for both create (Id = null) and edit (Id set)
    public int? Id { get; set; }

    [Required(ErrorMessage = "Category name is required.")]
    [MaxLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
    [Display(Name = "Category Name")]
    public string Name { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public bool IsEdit => Id.HasValue && Id > 0;
}
