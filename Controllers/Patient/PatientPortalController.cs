using System.Threading.Tasks;
using BE.Services;
using BE.Dtos.Patient;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

using Microsoft.AspNetCore.Authorization;

namespace BE.Controllers;

[Route("api/[controller]")]
[ApiController]
[EnableCors("CorsPolicy")]
[Authorize]
public class PatientPortalController : ControllerBase
{
    private readonly PatientPortalService _patientService;

    public PatientPortalController(PatientPortalService patientService)
    {
        _patientService = patientService;
    }

    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard([FromQuery] int? patientId)
    {
        var data = await _patientService.GetPatientDashboardDataAsync(patientId);
        if (data == null) return NotFound(new { message = "Không tìm thấy hồ sơ bệnh án điện tử." });
        return Ok(data);
    }

    [HttpGet("booking-options")]
    public async Task<IActionResult> GetBookingOptions()
    {
        var data = await _patientService.GetBookingOptionsAsync();
        return Ok(data);
    }

    [HttpPost("book-appointment")]
    public async Task<IActionResult> BookAppointment([FromBody] PatientBookingDto dto)
    {
        try
        {
            var res = await _patientService.BookAppointmentAsync(dto);
            if (res == null) return BadRequest(new { message = "Lỗi khi đăng ký lịch khám bệnh." });
            return Ok(res);
        }
        catch (System.InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("update-email")]
    public async Task<IActionResult> UpdateEmail([FromBody] UpdateEmailDto dto)
    {
        bool result = await _patientService.UpdateEmailAsync(dto.PatientId, dto.Email);
        if (!result)
        {
            return BadRequest(new { message = "Không thể cập nhật email. Email này có thể đã được sử dụng bởi tài khoản khác." });
        }
        return Ok(new { message = "Cập nhật email thành công!" });
    }
}
