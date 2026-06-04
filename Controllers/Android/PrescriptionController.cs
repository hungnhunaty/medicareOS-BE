using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using BE.Services;
using BE.Dtos.Android;

namespace BE.Controllers;

[Route("api/prescriptions")]
[ApiController]
[Authorize]
public class PrescriptionController : ControllerBase
{
    private readonly PrescriptionService _service;

    public PrescriptionController(PrescriptionService service)
    {
        _service = service;
    }

    [HttpGet("by-examination/{examId}")]
    public async Task<IActionResult> GetByExamination(int examId)
    {
        var userId = GetCurrentUserId();

        var result = await _service.GetByExaminationAsync(examId, userId);

        if (result == null)
            return NotFound();

        return Ok(result);
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdClaim))
            throw new UnauthorizedAccessException("User ID not found");
        return int.Parse(userIdClaim);
    }
}