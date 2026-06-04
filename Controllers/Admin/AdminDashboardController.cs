using System.Threading.Tasks;
using BE.Services;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

using Microsoft.AspNetCore.Authorization;

namespace BE.Controllers;

[Route("api/[controller]")]
[ApiController]
[EnableCors("CorsPolicy")]
[Authorize(Roles = "Admin")]
public class AdminDashboardController : ControllerBase
{
    private readonly AdminDashboardService _dashboardService;

    public AdminDashboardController(AdminDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        var data = await _dashboardService.GetDashboardStatsAsync();
        return Ok(data);
    }

    [HttpGet("active-queues")]
    public async Task<IActionResult> GetActiveQueues()
    {
        var data = await _dashboardService.GetActiveQueuesAsync();
        return Ok(data);
    }

    [HttpPut("update-status/{examId}")]
    public async Task<IActionResult> UpdateStatus(int examId, [FromBody] QueueStatusUpdateDto dto)
    {
        var result = await _dashboardService.UpdateQueueStatusAsync(examId, dto.Status);
        if (!result) return NotFound(new { message = "Không tìm thấy ca khám" });
        return Ok(new { message = "Cập nhật thành công" });
    }

    [HttpPost("emergency")]
    public async Task<IActionResult> CreateEmergencyExam([FromBody] EmergencyExamDto dto)
    {
        var res = await _dashboardService.CreateEmergencyExamAsync(dto.PatientName, dto.Phone, dto.Symptoms);
        if (res == null) return BadRequest(new { message = "Lỗi khởi tạo ca khám khẩn cấp" });
        return Ok(res);
    }
}

public class QueueStatusUpdateDto
{
    public string Status { get; set; } = "";
}

public class EmergencyExamDto
{
    public string PatientName { get; set; } = "";
    public string Phone { get; set; } = "";
    public string Symptoms { get; set; } = "";
}
