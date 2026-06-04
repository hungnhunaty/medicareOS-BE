using Microsoft.AspNetCore.Mvc;
using BE.Services;
using BE.Dtos.Android;
using Microsoft.AspNetCore.Authorization;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

namespace BE.Controllers;

[Route("api/invoices")]
[ApiController]
[Authorize]
public class InvoiceController : ControllerBase
{
    private readonly InvoiceService _service;

    public InvoiceController(InvoiceService service)
    {
        _service = service;
    }

    [HttpGet("by-examination/{examId}")]
    public async Task<IActionResult> GetAllByExamination(int examId)
    {
        var userId = GetCurrentUserId();

        var results = await _service.GetAllByExaminationAsync(examId, userId);

        if (results == null || results.Count == 0)
            return NotFound();

        return Ok(results);
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdClaim))
            throw new UnauthorizedAccessException("User ID not found");
        return int.Parse(userIdClaim);
    }
}