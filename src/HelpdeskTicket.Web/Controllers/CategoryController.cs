using HelpdeskTicket.Application.DTOs.Category;
using HelpdeskTicket.Core.Constants;
using HelpdeskTicket.Web.Helpers;
using HelpdeskTicket.Web.Services;
using HelpdeskTicket.Web.ViewModels.Category;
using Microsoft.AspNetCore.Mvc;

namespace HelpdeskTicket.Web.Controllers;

public class CategoryController : Controller
{
    private readonly ApiCategoryService _categoryService;
    private readonly ILogger<CategoryController> _logger;

    public CategoryController(ApiCategoryService categoryService, ILogger<CategoryController> logger)
    {
        _categoryService = categoryService;
        _logger = logger;
    }

    // GET /Category/Index — list + inline form
    //Admin sees ALL categories (active + inactive) — includeInactive = true
    public async Task<IActionResult> Index(int? editId = null)
    {
        if (!SessionHelper.IsLoggedIn(HttpContext.Session))
            return RedirectToAction("Login", "Account");
        if (SessionHelper.GetRoleId(HttpContext.Session) != Roles.AdminId)
            return RedirectToAction("Index", "Dashboard");

        // includeInactive: true so admin sees ALL rows in the grid
        var categories = await _categoryService.GetAllAsync(includeInactive: true);
        var model = new CategoryViewModel { Categories = categories };

        // If editId provided, pre-fill form for editing
        if (editId.HasValue)
        {
            var target = categories.FirstOrDefault(c => c.Id == editId);
            if (target is not null)
            {
                model.Id = target.Id;
                model.Name = target.Name;
                model.IsActive = target.IsActive;
            }
        }

        return View(model);
    }

    // POST /Category/Save — insert (Id null) or update (Id set)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(CategoryViewModel model)
    {
        if (!SessionHelper.IsLoggedIn(HttpContext.Session))
            return RedirectToAction("Login", "Account");
        if (SessionHelper.GetRoleId(HttpContext.Session) != Roles.AdminId)
            return RedirectToAction("Index", "Dashboard");

        if (!ModelState.IsValid)
        {
            model.Categories = await _categoryService.GetAllAsync(includeInactive: true);
            return View("Index", model);
        }

        var request = new SaveCategoryRequest
        {
            Id = model.Id,
            Name = model.Name.Trim(),
            IsActive = model.IsActive
        };

        var (success, message) = await _categoryService.SaveAsync(request);

        if (success) TempData["SuccessMessage"] = message;
        else TempData["ErrorMessage"] = message;

        return RedirectToAction(nameof(Index));
    }

    // POST /Category/Delete/{id} — soft delete via POST (not DELETE verb)
    // Using POST because browser forms don't support DELETE method
    //[HttpPost]
    //[ValidateAntiForgeryToken]
    //public async Task<IActionResult> Delete(int id)
    //{
    //    if (!SessionHelper.IsLoggedIn(HttpContext.Session))
    //        return RedirectToAction("Login", "Account");
    //    if (SessionHelper.GetRoleId(HttpContext.Session) != Roles.AdminId)
    //        return RedirectToAction("Index", "Dashboard");

    //    var (success, message) = await _categoryService.DeleteAsync(id);

    //    if (success) TempData["SuccessMessage"] = message;
    //    else TempData["ErrorMessage"] = message;

    //    return RedirectToAction(nameof(Index));
    //}
}
