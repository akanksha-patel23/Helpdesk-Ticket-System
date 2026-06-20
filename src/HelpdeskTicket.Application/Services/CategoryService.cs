using HelpdeskTicket.Application.DTOs.Category;
using HelpdeskTicket.Application.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelpdeskTicket.Application.Services;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _repo;
    private readonly ILogger<CategoryService> _logger;

    public CategoryService(ICategoryRepository repo, ILogger<CategoryService> logger)
    {
        _repo = repo;
        _logger = logger;
    }

    public async Task<List<CategoryDto>> GetAllAsync(bool includeInactive = false)
    {
        _logger.LogInformation("GetAllAsync categories called.");
        return await _repo.GetAllAsync(includeInactive);
    }
    public async Task<(bool Success, string Message)> SaveAsync(SaveCategoryRequest request)
    {
        _logger.LogInformation("SaveCategory Id:{Id} Name:{Name}", request.Id, request.Name);
        return await _repo.SaveAsync(request);
    }
    //public async Task<(bool Success, string Message)> DeleteAsync(int id)
    //{
    //    _logger.LogInformation("DeleteCategory Id:{Id}", id);
    //    return await _repo.DeleteAsync(id);
    //}
}
