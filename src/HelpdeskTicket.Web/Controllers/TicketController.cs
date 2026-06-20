using HelpdeskTicket.Web.Helpers;
using HelpdeskTicket.Web.Services;
using HelpdeskTicket.Web.ViewModels.Ticket;
using Microsoft.AspNetCore.Mvc;

namespace HelpdeskTicket.Web.Controllers;

// TODO: Implement in step-by-step coding phase
public class TicketController : Controller
{
    private readonly ApiTicketService _ticketService;
    private readonly ApiCommentService _commentService;
    private readonly ApiUserService _userService;
    private readonly ILogger<TicketController> _logger;

    public TicketController(ApiTicketService ticketService, ApiCommentService commentService, ApiUserService userService, ILogger<TicketController> logger)
    {
        _ticketService = ticketService;
        _commentService = commentService;
        _userService = userService;
        _logger = logger;
    }
    // GET /Ticket/Index — paginated, filterable list
    public async Task<IActionResult> Index(
        string? searchText = null,
        int? statusId = null,
        int? priorityId = null,
        int? categoryId = null,
        int pageNumber = 1,
        int pageSize = 10)
    {
        if (!SessionHelper.IsLoggedIn(HttpContext.Session))
            return RedirectToAction("Login", "Account");

        var model = new TicketListViewModel
        {
            SearchText = searchText,
            StatusId = statusId,
            PriorityId = priorityId,
            CategoryId = categoryId,
            PageNumber = pageNumber < 1 ? 1 : pageNumber,
            PageSize = pageSize,
            Statuses = ApiTicketService.GetStatusSelectList(),
            Priorities = ApiTicketService.GetPrioritySelectList(),
            Categories = await _ticketService.GetCategorySelectListAsync()
        };

        var result = await _ticketService.GetTicketListAsync(model);

        if (result is null)
        {
            TempData["ErrorMessage"] = "Could not load tickets. Please try again.";
            model.PagedResult = new HelpdeskTicket.Core.Wrappers.PagedResponse<HelpdeskTicket.Application.DTOs.Ticket.TicketListDto>();
        }
        else
        {
            model.PagedResult = result;
        }

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Update(int id, int? displayNumber)
    {
        if (!SessionHelper.IsLoggedIn(HttpContext.Session))
            return RedirectToAction("Login", "Account");

        var ticket = await _ticketService.GetTicketByIdAsync(id);

        if (ticket is null)
        {
            TempData["ErrorMessage"] = "Ticket not found or you don't have access.";
            return RedirectToAction(nameof(Index));
        }

        var model = new TicketDetailViewModel
        {
            Ticket = ticket,
            Statuses = ApiTicketService.GetStatusSelectList(),
            Priorities = ApiTicketService.GetPrioritySelectList(),
            Developers = await _userService.GetDeveloperSelectListAsync()
        };

        return View(model);
    }

    // POST /Ticket/Update — Admin/Developer update status/priority/assignment
    //[HttpPost]
    //[ValidateAntiForgeryToken]
    //public async Task<IActionResult> Update(int ticketId, int? statusId,
    //    int? priorityId, int? assignedToId)
    //{
    //    if (!SessionHelper.IsLoggedIn(HttpContext.Session))
    //        return RedirectToAction("Login", "Account");

    //    var (success, message) = await _ticketService.UpdateTicketAsync(
    //        ticketId, statusId, priorityId, assignedToId);

    //    if (success)
    //        TempData["SuccessMessage"] = message;
    //    else
    //        TempData["ErrorMessage"] = message;

    //    return RedirectToAction(nameof(Details), new { id = ticketId });
    //}

    //[HttpPost]
    //[ValidateAntiForgeryToken]
    //public async Task<IActionResult> Update(
    //int ticketId, int? statusId, int? priorityId,
    //int? assignedToId, IFormFile? attachment, int? displayNumber)
    //{
    //    if (!SessionHelper.IsLoggedIn(HttpContext.Session))
    //        return RedirectToAction("Login", "Account");

    //    // Validate attachment if provided
    //    if (attachment is not null && attachment.Length > 0)
    //    {
    //        var allowedExt = new[] { ".pdf", ".png", ".jpeg", ".jpg" };
    //        var ext = Path.GetExtension(attachment.FileName).ToLowerInvariant();

    //        if (!allowedExt.Contains(ext))
    //        {
    //            TempData["ErrorMessage"] = "Invalid file type. Allowed: PDF, PNG, JPEG, JPG.";
    //            //return RedirectToAction(nameof(Details), new { id = ticketId });
    //            return RedirectToAction(nameof(Details), new { id = ticketId, displayNumber = displayNumber });
    //        }
    //        if (attachment.Length > 5_242_880)
    //        {
    //            TempData["ErrorMessage"] = "File exceeds the 5 MB size limit.";
    //            //return RedirectToAction(nameof(Details), new { id = ticketId });
    //            return RedirectToAction(nameof(Details), new { id = ticketId, displayNumber = displayNumber });
    //        }
    //    }

    //    var (success, message) = await _ticketService.UpdateTicketAsync(
    //        ticketId, statusId, priorityId, assignedToId, attachment);

    //    if (success) TempData["SuccessMessage"] = message;
    //    else TempData["ErrorMessage"] = message;

    //    //return RedirectToAction(nameof(Details), new { id = ticketId });
    //    return RedirectToAction(nameof(Details), new { id = ticketId, displayNumber = displayNumber });
    //}
    // Update POST — now includes Title + Description
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(
        int ticketId, string? title, string? description,
        int? statusId, int? priorityId, int? assignedToId, int? displayNumber)
    {
        if (!SessionHelper.IsLoggedIn(HttpContext.Session))
            return RedirectToAction("Login", "Account");

        var (success, message) = await _ticketService.UpdateTicketAsync(
            ticketId, title, description, statusId, priorityId, assignedToId);

        if (success) TempData["SuccessMessage"] = message;
        else TempData["ErrorMessage"] = message;

        //return RedirectToAction(nameof(Details), new { id = ticketId });
        return RedirectToAction(nameof(Details), new { id = ticketId, displayNumber });
    }

    // Add Attachment POST
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddAttachment(int ticketId, IFormFile attachment, int? displayNumber)
    {
        if (!SessionHelper.IsLoggedIn(HttpContext.Session))
            return RedirectToAction("Login", "Account");

        if (attachment is null || attachment.Length == 0)
        {
            TempData["ErrorMessage"] = "Please select a file.";
            return RedirectToAction(nameof(Details), new { id = ticketId });
        }

        var allowedExt = new[] { ".pdf", ".png", ".jpeg", ".jpg" };
        var ext = Path.GetExtension(attachment.FileName).ToLowerInvariant();

        if (!allowedExt.Contains(ext))
        {
            TempData["ErrorMessage"] = "Invalid file type. Allowed: PDF, PNG, JPEG, JPG.";
            return RedirectToAction(nameof(Details), new { id = ticketId });
        }
        if (attachment.Length > 5_242_880)
        {
            TempData["ErrorMessage"] = "File exceeds the 5 MB size limit.";
            return RedirectToAction(nameof(Details), new { id = ticketId });
        }

        var (success, message) = await _ticketService.AddAttachmentAsync(ticketId, attachment);

        if (success) TempData["SuccessMessage"] = message;
        else TempData["ErrorMessage"] = message;

        //return RedirectToAction(nameof(Details), new { id = ticketId });
        return RedirectToAction(nameof(Details), new { id = ticketId, displayNumber });
    }

    // Delete Attachment POST
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteAttachment(int attachmentId, int ticketId, int? displayNumber)
    {
        if (!SessionHelper.IsLoggedIn(HttpContext.Session))
            return RedirectToAction("Login", "Account");

        var (success, message) = await _ticketService.DeleteAttachmentAsync(attachmentId);

        if (success) TempData["SuccessMessage"] = message;
        else TempData["ErrorMessage"] = message;

        //return RedirectToAction(nameof(Details), new { id = ticketId });
        return RedirectToAction(nameof(Details), new { id = ticketId, displayNumber });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateTitleDescription(
    int ticketId, string title, string description, int? displayNumber)
    {
        if (!SessionHelper.IsLoggedIn(HttpContext.Session))
            return RedirectToAction("Login", "Account");

        if (string.IsNullOrWhiteSpace(title))
        {
            TempData["ErrorMessage"] = "Title cannot be empty.";
            return RedirectToAction(nameof(Details), new { id = ticketId });
        }
        var originalTicket = await _ticketService.GetTicketByIdAsync(ticketId);

        var (success, message) = await _ticketService.UpdateTicketAsync(
            ticketId,
            title.Trim(),
            description?.Trim(),
            null, null, null);   // status/priority/assignment unchanged

        //if (success)
        //    TempData["TitleUpdateSuccess"] = "Title and description updated successfully.";
        //else
        //    TempData["ErrorMessage"] = message;

        if (success)
        {
            bool titleChanged = title?.Trim() != originalTicket?.Title;
            bool descChanged = description?.Trim() != originalTicket?.Description;

            if (titleChanged && descChanged)
                TempData["SuccessMessage"] = "Title and description updated successfully.";
            else if (titleChanged)
                TempData["SuccessMessage"] = "Title updated successfully.";
            else if (descChanged)
                TempData["SuccessMessage"] = "Description updated successfully.";
            else
                TempData["InfoMessage"] = "No changes detected.";
        }
        else
        {
            TempData["ErrorMessage"] = message;
        }
        //return RedirectToAction(nameof(Details), new { id = ticketId });
        return RedirectToAction(nameof(Details), new { id = ticketId, displayNumber });
    }

    // GET /Ticket/Create
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        if (!SessionHelper.IsLoggedIn(HttpContext.Session))
            return RedirectToAction("Login", "Account");

        var model = new CreateTicketViewModel();
        var (categories, priorities) = await _ticketService.GetSelectListsAsync();
        model.Categories = categories;
        model.Priorities = priorities;
        return View(model);
    }

    // GET /Ticket/Details/{id}
    [HttpGet]
    public async Task<IActionResult> Details(int id, int? displayNumber)
    {
        ViewBag.DisplayNumber = displayNumber;
        if (!SessionHelper.IsLoggedIn(HttpContext.Session))
            return RedirectToAction("Login", "Account");

        var ticket = await _ticketService.GetTicketByIdAsync(id);

        if (ticket is null)
        {
            TempData["ErrorMessage"] = "Ticket not found or you don't have access.";
            return RedirectToAction(nameof(Index));
        }

        var roleId = SessionHelper.GetRoleId(HttpContext.Session);

        var model = new TicketDetailViewModel
        {
            Ticket = ticket,
            Statuses = ApiTicketService.GetStatusSelectList(),
            Priorities = ApiTicketService.GetPrioritySelectList(),
            Developers = await _userService.GetDeveloperSelectListAsync()  // ADD THIS
        };

        // Developers list only needed for Admin
        //if (roleId == HelpdeskTicket.Core.Constants.Roles.AdminId)
        //    model.Developers = ApiTicketService.GetStatusSelectList(); // replaced below

        return View(model);
    }

    // POST /Ticket/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateTicketViewModel model)
    {
        if (!SessionHelper.IsLoggedIn(HttpContext.Session))
            return RedirectToAction("Login", "Account");

        // Server-side file validation in Web layer before sending to API
        if (model.Attachment is not null && model.Attachment.Length > 0)
        {
            var allowedExt = new[] { ".pdf", ".png", ".jpeg", ".jpg" };
            var ext = Path.GetExtension(model.Attachment.FileName).ToLowerInvariant();

            if (!allowedExt.Contains(ext))
                ModelState.AddModelError("Attachment", "Invalid file type. Allowed: PDF, PNG, JPEG, JPG.");

            if (model.Attachment.Length > 5_242_880)
                ModelState.AddModelError("Attachment", "File exceeds the 5 MB size limit.");
        }

        if (!ModelState.IsValid)
        {
            var (categories, priorities) = await _ticketService.GetSelectListsAsync();
            model.Categories = categories;
            model.Priorities = priorities;
            return View(model);
        }

        var (success, message) = await _ticketService.CreateTicketAsync(model);

        if (!success)
        {
            ModelState.AddModelError(string.Empty, message);
            var (categories, priorities) = await _ticketService.GetSelectListsAsync();
            model.Categories = categories;
            model.Priorities = priorities;
            return View(model);
        }

        TempData["SuccessMessage"] = "Ticket created successfully!";
        return RedirectToAction("Index", "Dashboard");
    }

    // POST /Ticket/AddComment
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddComment(int ticketId, string commentBody, int? displayNumber)
    {
        if (!SessionHelper.IsLoggedIn(HttpContext.Session))
            return RedirectToAction("Login", "Account");

        if (string.IsNullOrWhiteSpace(commentBody) || commentBody.Length > 1000)
        {
            TempData["ErrorMessage"] = "Comment must be between 1 and 1000 characters.";
            return RedirectToAction(nameof(Details), new { id = ticketId });
        }

        var (success, message) = await _commentService.AddCommentAsync(ticketId, commentBody);

        if (success)
            TempData["SuccessMessage"] = "Comment added.";
        else
            TempData["ErrorMessage"] = message;

        //return RedirectToAction(nameof(Details), new { id = ticketId });
        return RedirectToAction(nameof(Details), new { id = ticketId, displayNumber });
    }

    // Stub — fully built in Ticket List module
    //public IActionResult Index()
    //{
    //    if (!SessionHelper.IsLoggedIn(HttpContext.Session))
    //        return RedirectToAction("Login", "Account");

    //    return View();
    //}

    // Stub — fully built in Ticket Detail module
    //public IActionResult Details(int id)
    //{
    //    if (!SessionHelper.IsLoggedIn(HttpContext.Session))
    //        return RedirectToAction("Login", "Account");

    //    return View();
    //}
}
