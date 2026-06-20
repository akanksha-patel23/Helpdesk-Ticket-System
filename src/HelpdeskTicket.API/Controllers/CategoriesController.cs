using HelpdeskTicket.Application.DTOs.Category;
using HelpdeskTicket.Application.Interfaces;
using HelpdeskTicket.Core.Wrappers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HelpdeskTicket.API.Controllers;

[Authorize]
[ApiController]
[Route("api/categories")]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _categoryService;
    private readonly ILogger<CategoriesController> _logger;

    public CategoriesController(ICategoryService categoryService, ILogger<CategoriesController> logger)
    {
        _categoryService = categoryService;
        _logger = logger;
    }

    // GET api/categories?includeInactive=false  — ticket form: active only
    // GET api/categories?includeInactive=true   — admin page: all
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool includeInactive = false)
    {
        var categories = await _categoryService.GetAllAsync(includeInactive);
        return Ok(ApiResponse<List<CategoryDto>>.Ok(categories));
    }

    // POST api/categories  — insert (Id null) or update (Id set)
    [HttpPost]
    public async Task<IActionResult> Save([FromBody] SaveCategoryRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<object>.Fail("Validation failed."));

        var (success, message) = await _categoryService.SaveAsync(request);
        if (!success) return BadRequest(ApiResponse<object>.Fail(message));
        return Ok(ApiResponse<object>.Ok(new { }, message));
    }
    // DELETE api/categories/{id} — soft delete (sets IsActive = false)
    //[HttpDelete("{id:int}")]
    //public async Task<IActionResult> Delete(int id)
    //{
    //    var (success, message) = await _categoryService.DeleteAsync(id);
    //    if (!success) return BadRequest(ApiResponse<object>.Fail(message));
    //    return Ok(ApiResponse<object>.Ok(new { }, message));
    //}
}
