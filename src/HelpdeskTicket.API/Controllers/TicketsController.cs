using HelpdeskTicket.Application.DTOs.Ticket;
using HelpdeskTicket.Application.Interfaces;
using HelpdeskTicket.Core.Helpers;
using HelpdeskTicket.Core.Settings;
using HelpdeskTicket.Core.Wrappers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace HelpdeskTicket.API.Controllers;

// TODO: Implement in step-by-step coding phase
[Authorize]
[ApiController]
[Route("api/tickets")]
public class TicketsController : ControllerBase
{
    private readonly ITicketService _ticketService;
    private readonly FileUploadSettings _fileSettings;
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<TicketsController> _logger;

    public TicketsController(
        ITicketService ticketService,
        IOptions<FileUploadSettings> fileSettings,
        IWebHostEnvironment env,
        ILogger<TicketsController> logger)
    {
        _ticketService = ticketService;
        _fileSettings = fileSettings.Value;
        _env = env;
        _logger = logger;
    }
    // GET api/tickets?userId=1&roleId=1&searchText=&statusId=&priorityId=&pageNumber=1&pageSize=10
    [HttpGet]
    public async Task<IActionResult> GetTicketList(
        [FromQuery] int userId,
        [FromQuery] int roleId,
        [FromQuery] string? searchText = null,
        [FromQuery] int? statusId = null,
        [FromQuery] int? priorityId = null,
        [FromQuery] int? categoryId = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        if (userId <= 0 || roleId <= 0)
            return BadRequest(ApiResponse<object>.Fail("Invalid userId or roleId."));

        var filter = new TicketFilterRequest
        {
            RequestedById = userId,
            RequestedRoleId = roleId,
            SearchText = searchText,
            StatusId = statusId,
            PriorityId = priorityId,
            CategoryId = categoryId,
            PageNumber = pageNumber < 1 ? 1 : pageNumber,
            PageSize = pageSize < 1 ? 10 : pageSize > 100 ? 100 : pageSize
        };

        var result = await _ticketService.GetTicketListAsync(filter);
        return Ok(result);
    }

    // GET api/tickets/{id}
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetTicketById(
        int id, [FromQuery] int userId, [FromQuery] int roleId)
    {
        if (userId <= 0 || roleId <= 0)
            return BadRequest(ApiResponse<object>.Fail("Invalid userId or roleId."));

        var ticket = await _ticketService.GetTicketByIdAsync(id, userId, roleId);

        if (ticket is null)
            return NotFound(ApiResponse<object>.Fail("Ticket not found or access denied."));

        return Ok(ApiResponse<TicketDetailDto>.Ok(ticket));
    }

    // GET api/tickets/dropdowns
    [HttpGet("dropdowns")]
    public async Task<IActionResult> GetDropdowns()
    {
        var dropdowns = await _ticketService.GetDropdownsAsync();
        return Ok(ApiResponse<TicketDropdownsDto>.Ok(dropdowns));
    }

    // POST api/tickets — multipart/form-data
    [HttpPost]
    [RequestSizeLimit(10_485_760)]
    public async Task<IActionResult> CreateTicket([FromForm] CreateTicketRequest request, IFormFile? attachment)
    {
        if (!ModelState.IsValid)
            return BadRequest(ApiResponse<object>.Fail("Validation failed."));

        // ?? File upload ???????????????????????????????????????????
        if (attachment is not null && attachment.Length > 0)
        {
            if (!FileHelper.IsAllowedExtension(attachment.FileName))
                return BadRequest(ApiResponse<object>.Fail(
                    "Invalid file type. Allowed: PDF, PNG, JPEG, JPG."));

            if (!FileHelper.IsAllowedSize(attachment.Length))
                return BadRequest(ApiResponse<object>.Fail(
                    "File size exceeds the 5 MB limit."));

            // PhysicalUploadPath in appsettings can point to Web's wwwroot/uploads.
            // Leave empty to save inside the API's own wwwroot/uploads.
            var uploadsFolder = !string.IsNullOrWhiteSpace(_fileSettings.PhysicalUploadPath)
                ? _fileSettings.PhysicalUploadPath
                : Path.Combine(_env.WebRootPath ?? _env.ContentRootPath, _fileSettings.UploadFolder);

            Directory.CreateDirectory(uploadsFolder);

            var uniqueName = FileHelper.GenerateUniqueFileName(attachment.FileName);
            var physicalPath = Path.Combine(uploadsFolder, uniqueName);

            await using (var stream = new FileStream(physicalPath, FileMode.Create))
                await attachment.CopyToAsync(stream); //saves to wwwroot

            request.AttachmentPath = FileHelper.GetVirtualPath(_fileSettings.UploadFolder, uniqueName);
            _logger.LogInformation("Attachment saved: {Path}", request.AttachmentPath);
        }
        var (success, message, ticketId) = await _ticketService.CreateTicketAsync(request);
        _logger.LogInformation("CreateTicket result: Success={S} Message={M} TicketId={Id}", success, message, ticketId);

        if (!success)
            return BadRequest(ApiResponse<object>.Fail(message));

        // Also insert into tblTicketAttachment for initial attachment
        //if (attachment is not null && attachment.Length > 0 && !string.IsNullOrWhiteSpace(request.AttachmentPath))
        //{
        //    var fileName = Path.GetFileName(attachment.FileName);
        //    await _ticketService.AddAttachmentAsync(ticketId, request.AttachmentPath, fileName, request.CreatedById);
        //}

        return Ok(ApiResponse<object>.Ok(new { TicketId = ticketId }, message));
    }
    // PUT api/tickets/{id}
    //[HttpPut("{id:int}")]
    //public async Task<IActionResult> UpdateTicket(int id, [FromBody] UpdateTicketRequest request)
    //{
    //    if (id != request.TicketId)
    //        return BadRequest(ApiResponse<object>.Fail("Ticket ID mismatch."));

    //    var (success, message) = await _ticketService.UpdateTicketAsync(request);
    //    if (!success) return BadRequest(ApiResponse<object>.Fail(message));
    //    return Ok(ApiResponse<object>.Ok(new { }, message));
    //}

    // PUT api/tickets/{id} — updated to include Title + Description
    [HttpPut("{id:int}")]
    [RequestSizeLimit(10_485_760)]
    public async Task<IActionResult> UpdateTicket(int id, [FromForm] UpdateTicketRequest request, IFormFile? attachment)
    {
        if (id != request.TicketId)
            return BadRequest(ApiResponse<object>.Fail("Ticket ID mismatch."));

        // Handle new attachment if uploaded
        //if (attachment is not null && attachment.Length > 0)
        //{
        //    if (!FileHelper.IsAllowedExtension(attachment.FileName))
        //        return BadRequest(ApiResponse<object>.Fail(
        //            "Invalid file type. Allowed: PDF, PNG, JPEG, JPG."));

        //    if (!FileHelper.IsAllowedSize(attachment.Length))
        //        return BadRequest(ApiResponse<object>.Fail("File exceeds the 5 MB limit."));

        //    var folder = !string.IsNullOrWhiteSpace(_fileSettings.PhysicalUploadPath)
        //        ? _fileSettings.PhysicalUploadPath
        //        : Path.Combine(_env.WebRootPath ?? _env.ContentRootPath, _fileSettings.UploadFolder);

        //    Directory.CreateDirectory(folder);
        //    var uniqueName = FileHelper.GenerateUniqueFileName(attachment.FileName);
        //    var physicalPath = Path.Combine(folder, uniqueName);

        //    await using (var stream = new FileStream(physicalPath, FileMode.Create))
        //        await attachment.CopyToAsync(stream);

        //    request.AttachmentPath = FileHelper.GetVirtualPath(_fileSettings.UploadFolder, uniqueName);
        //    _logger.LogInformation("Attachment updated: {Path}", request.AttachmentPath);
        //}

        var (success, message) = await _ticketService.UpdateTicketAsync(request);
        if (!success) return BadRequest(ApiResponse<object>.Fail(message));
        return Ok(ApiResponse<object>.Ok(new { }, message));
    }

    // POST api/tickets/{id}/attachments — add new attachment
    [HttpPost("{id:int}/attachments")]
    [RequestSizeLimit(10_485_760)]
    public async Task<IActionResult> AddAttachment(int id,
        [FromQuery] int userId, IFormFile attachment)
    {
        if (attachment is null || attachment.Length == 0)
            return BadRequest(ApiResponse<object>.Fail("No file provided."));

        if (!FileHelper.IsAllowedExtension(attachment.FileName))
            return BadRequest(ApiResponse<object>.Fail("Invalid file type."));

        if (!FileHelper.IsAllowedSize(attachment.Length))
            return BadRequest(ApiResponse<object>.Fail("File exceeds 5 MB limit."));

        var folder = !string.IsNullOrWhiteSpace(_fileSettings.PhysicalUploadPath)
            ? _fileSettings.PhysicalUploadPath
            : Path.Combine(_env.WebRootPath ?? _env.ContentRootPath, _fileSettings.UploadFolder);

        Directory.CreateDirectory(folder);
        var uniqueName = FileHelper.GenerateUniqueFileName(attachment.FileName);
        var physicalPath = Path.Combine(folder, uniqueName);

        await using (var stream = new FileStream(physicalPath, FileMode.Create))
            await attachment.CopyToAsync(stream);

        var virtualPath = FileHelper.GetVirtualPath(_fileSettings.UploadFolder, uniqueName);

        var (success, message) = await _ticketService.AddAttachmentAsync(
            id, virtualPath, attachment.FileName, userId);

        if (!success) return BadRequest(ApiResponse<object>.Fail(message));
        return Ok(ApiResponse<object>.Ok(new { }, message));
    }

    // DELETE api/tickets/attachments/{attachmentId}?userId=X
    [HttpDelete("attachments/{attachmentId:int}")]
    public async Task<IActionResult> DeleteAttachment(
        int attachmentId, [FromQuery] int userId)
    {
        var (success, message) = await _ticketService.DeleteAttachmentAsync(attachmentId, userId);
        if (!success) return BadRequest(ApiResponse<object>.Fail(message));
        return Ok(ApiResponse<object>.Ok(new { }, message));
    }
}
