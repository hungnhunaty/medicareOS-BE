using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using BE.Services;
using BE.Dtos.Android;

namespace BE.Controllers;

[Route("api/patient")]
[ApiController]
[Authorize]
public class PatientController : ControllerBase
{
    private readonly PatientService _patientService;
    private readonly QueueService _queueService;

    public PatientController(PatientService patientService, QueueService queueService)
    {
        _patientService = patientService;
        _queueService = queueService;
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetMyProfile()
    {
        var userId = GetCurrentUserId();

        var result = await _patientService.GetMyProfileAsync(userId);

        if (result == null)
            return NotFound();

        return Ok(result);
    }

    [HttpGet("queue")]
    public async Task<IActionResult> GetMyQueue()
    {
        var userId = GetCurrentUserId();

        var result = await _queueService.GetPatientQueueAsync(userId);

        if (result == null)
            return NotFound(new { message = "Không có hàng đợi nào đang hoạt động" });

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