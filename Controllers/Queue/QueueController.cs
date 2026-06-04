using System.Threading.Tasks;
using BE.Dtos.Queue;
using BE.Dtos.Qr;
using BE.Services;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

using Microsoft.AspNetCore.Authorization;

namespace BE.Controllers;

[Route("api/[controller]")]
[ApiController]
[EnableCors("CorsPolicy")]
[Authorize(Roles = "Admin,Doctor,Staff")]
public class QueueController : ControllerBase
{
    private readonly QueueService _queueService;
    private readonly QueueQrService _queueQrService;

    public QueueController(QueueService queueService, QueueQrService queueQrService)
    {
        _queueService = queueService;
        _queueQrService = queueQrService;
    }

    [HttpPost("add")]
    public async Task<IActionResult> AddToQueue([FromBody] QueueRequestDto dto)
    {
        var result = await _queueService.AddToQueueAsync(dto);
        if (result == null)
            return BadRequest(new { message = "Không thể thêm vào hàng đợi. Vui lòng kiểm tra lại thông tin khoa hoặc bệnh nhân." });

        return Ok(result);
    }

    [HttpPost("qr/create")]
    public async Task<IActionResult> CreateQrSession([FromBody] QueueQrCreateDto dto)
    {
        try
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier) 
                           ?? User.FindFirst("sub");
            
            if (userIdClaim == null)
                return Unauthorized(new { message = "Token không chứa user claim. Vui lòng đăng nhập lại." });

            if (!int.TryParse(userIdClaim.Value, out var staffUserId))
                return BadRequest(new { message = "User ID claim không hợp lệ." });

            var result = await _queueQrService.CreateQueueSessionAsync(dto, staffUserId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Lỗi tạo QR: " + ex.Message });
        }
    }

    [HttpGet("status/{departmentId}")]
    public async Task<IActionResult> GetDepartmentQueues(int departmentId)
    {
        var result = await _queueService.GetClinicQueuesAsync(departmentId);
        return Ok(result);
    }
}
