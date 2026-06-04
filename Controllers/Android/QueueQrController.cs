using BE.Dtos.Qr;
using BE.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BE.Controllers;

[Route("api/queue/qr")]
[ApiController]
public class QueueQrController : ControllerBase
{
    private readonly QueueQrService _queueQrService;

    public QueueQrController(QueueQrService queueQrService)
    {
        _queueQrService = queueQrService;
    }

    [HttpPost("checkin")]
    [Authorize]
    public async Task<IActionResult> CheckinQr([FromBody] QueueQrRequestDto dto)
    {
        // lấy patient từ JWT
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value
            ?? User.FindFirst("userId")?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var patientUserId))
            return Unauthorized(new { message = "Token không chứa userId hợp lệ." });

        var result = await _queueQrService.CheckinByQrAsync(dto, patientUserId);

        return Ok(result);
    }
}