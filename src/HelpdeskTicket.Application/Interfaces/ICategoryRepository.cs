using HelpdeskTicket.Application.DTOs.Category;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelpdeskTicket.Application.Interfaces;

public interface ICategoryRepository
{
    Task<List<CategoryDto>> GetAllAsync(bool includeInactive = false);
    Task<(bool Success, string Message)> SaveAsync(SaveCategoryRequest request);
    //Task<(bool Success, string Message)> DeleteAsync(int id);
}
